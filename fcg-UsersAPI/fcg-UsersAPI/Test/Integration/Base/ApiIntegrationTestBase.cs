
using Application.Dto.Response;
using Test.Integration.Base.Model.Results;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Test.Integration.Base.Model.Results;
using Xunit;

namespace Test.Integration.Base;

/// <summary>
/// Classe base para testes de integração da API
/// Configura o ambiente Aspire e fornece métodos auxiliares
/// </summary>
public abstract class ApiIntegrationTestBase : IAsyncLifetime
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(120);
    protected DistributedApplication? App;
    protected HttpClient ApiClient = default!;

    // Opções padrão para deserialização JSON (case-insensitive)
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async ValueTask InitializeAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        // Cria o Aspire AppHost
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.FCG_Host>(cancellationToken);

        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Information);
            logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
        });

        // Build e Start da aplicação
        App = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await App.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Aguarda recursos estarem prontos
        await App.ResourceNotifications.WaitForResourceHealthyAsync("DbFcg", cancellationToken)
       .WaitAsync(DefaultTimeout, cancellationToken);

        await App.ResourceNotifications.WaitForResourceHealthyAsync("fcg-api", cancellationToken)
       .WaitAsync(DefaultTimeout, cancellationToken);

        // ✅ Obtém endpoint HTTPS explicitamente
        var httpsEndpoint = App.GetEndpoint("fcg-api", "https");
        Console.WriteLine($"[DEBUG] HTTPS Endpoint: {httpsEndpoint}");

        // ✅ Cria HttpClient com handler customizado que aceita certificados SSL auto-assinados
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        ApiClient = new HttpClient(handler)
        {
            BaseAddress = httpsEndpoint, // Uri não precisa de conversão
            Timeout = DefaultTimeout
        };

        Console.WriteLine($"[DEBUG] API Base URL configurado: {ApiClient.BaseAddress}");
    }

    public async ValueTask DisposeAsync()
    {
        ApiClient?.Dispose();
        if (App != null)
        {
            await App.DisposeAsync();
        }
    }

    /// <summary>
    /// Define o header de autorização com token JWT
    /// </summary>
    protected void SetAuthorizationHeader(string token)
    {
        var cleanToken = token?.Trim();

        if (string.IsNullOrWhiteSpace(cleanToken))
        {
            throw new ArgumentException("Token não pode ser vazio ou nulo", nameof(token));
        }

        if (!IsValidJwtFormat(cleanToken))
        {
            throw new ArgumentException($"Token com formato inválido...", nameof(token));
        }

        // ✅ FIX: Usa cleanToken ao invés de token
        ApiClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", cleanToken);

        Console.WriteLine($"[DEBUG] Token configurado: {cleanToken.Substring(0, Math.Min(50, cleanToken.Length))}...");
        Console.WriteLine($"[DEBUG] Authorization Header: {ApiClient.DefaultRequestHeaders.Authorization}");
        Console.WriteLine($"[DEBUG] Authorization Scheme: {ApiClient.DefaultRequestHeaders.Authorization?.Scheme}");
        Console.WriteLine($"[DEBUG] Authorization Parameter: {ApiClient.DefaultRequestHeaders.Authorization?.Parameter?.Substring(0, Math.Min(50, ApiClient.DefaultRequestHeaders.Authorization?.Parameter?.Length ?? 0))}...");
    }

    /// <summary>
    /// Remove o header de autorização
    /// </summary>
    protected void ClearAuthorizationHeader()
    {
        ApiClient.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// Faz uma requisição GET e retorna o resultado tipado
    /// </summary>
    protected async Task<TResponse?> GetAsync<TResponse>(string url)
    {
        var response = await ApiClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions);
    }

    /// <summary>
    /// Faz uma requisição GET com envio explícito de headers
    /// </summary>
    protected async Task<HttpResponseMessage> GetWithAuthAsync(string url, CancellationToken cancellationToken = default)
    {
        // Cria requisição explícita para garantir que headers sejam enviados
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        // Copia explicitamente o Authorization header
        if (ApiClient.DefaultRequestHeaders.Authorization != null)
        {
            request.Headers.Authorization = ApiClient.DefaultRequestHeaders.Authorization;
            Console.WriteLine($"[DEBUG] Request Authorization Header: {request.Headers.Authorization}");
        }
        else
        {
            Console.WriteLine($"[DEBUG] WARNING: No Authorization header in ApiClient.DefaultRequestHeaders");
        }

        return await ApiClient.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Faz uma requisição POST e retorna a resposta
    /// </summary>
    protected async Task<HttpResponseMessage> PostAsync<TRequest>(string url, TRequest data)
    {
        return await ApiClient.PostAsJsonAsync(url, data);
    }

    /// <summary>
    /// Faz uma requisição PUT e retorna a resposta
    /// </summary>
    protected async Task<HttpResponseMessage> PutAsync<TRequest>(string url, TRequest data)
    {
        return await ApiClient.PutAsJsonAsync(url, data);
    }

    /// <summary>
    /// Faz uma requisição DELETE e retorna a resposta
    /// </summary>
    protected async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        return await ApiClient.DeleteAsync(url);
    }

    /// <summary>
    /// Deserializa resposta HTTP envolvida em ServiceResult
    /// Extrai automaticamente a propriedade Data do wrapper
    /// </summary>
    /// <typeparam name="T">Tipo do dado dentro de ServiceResult</typeparam>
    /// <param name="response">Resposta HTTP</param>
    /// <returns>Dados extraídos ou null se falhar</returns>
    protected async Task<T?> ReadServiceResultAsync<T>(HttpResponseMessage response)
    {
        var serviceResult = await response.Content.ReadFromJsonAsync<ServiceResult<T>>(JsonOptions);

        if (serviceResult == null)
        {
            throw new InvalidOperationException("ServiceResult retornou null");
        }

        if (!serviceResult.Succeeded)
        {
            var errors = string.Join(", ", serviceResult.Errors);
            throw new InvalidOperationException($"Operação falhou: {errors}");
        }

        return serviceResult.Data;
    }

    /// <summary>
    /// Deserializa resposta HTTP envolvida em ServiceResult com validação opcional
    /// Permite processar erros sem lançar exceção
    /// </summary>
    /// <typeparam name="T">Tipo do dado dentro de ServiceResult</typeparam>
    /// <param name="response">Resposta HTTP</param>
    /// <param name="throwOnError">Se true, lança exceção em caso de erro</param>
    /// <returns>ServiceResult completo para inspeção</returns>
    protected async Task<ServiceResult<T>> ReadServiceResultWithErrorsAsync<T>(
        HttpResponseMessage response,
        bool throwOnError = false)
    {
        var serviceResult = await response.Content.ReadFromJsonAsync<ServiceResult<T>>(JsonOptions);

        if (serviceResult == null)
        {
            throw new InvalidOperationException("ServiceResult retornou null");
        }

        if (throwOnError && !serviceResult.Succeeded)
        {
            var errors = string.Join(", ", serviceResult.Errors);
            throw new InvalidOperationException($"Operação falhou: {errors}");
        }

        return serviceResult;
    }

    /// <summary>
    /// Valida o formato do token JWT
    /// </summary>
    protected bool IsValidJwtFormat(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var parts = token.Split('.');
        return parts.Length == 3; // Header.Payload.Signature
    }
}
