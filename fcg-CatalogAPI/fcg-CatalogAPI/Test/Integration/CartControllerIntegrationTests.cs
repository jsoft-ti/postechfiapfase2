using Bogus;
using Application.Dto.Filter;
using Application.Dto.Order;
using Application.Dto.Request;
using Application.Dto.Response;
using Application.Dto.Result;
using Tests.Integration.Base;
using System.Net.Http.Json;

namespace Tests.Integration;

/// <summary>
/// Testes de integra��o para o CartController
/// Testa todos os endpoints de gerenciamento do carrinho de compras
/// </summary>
public class CartControllerIntegrationTests : ApiIntegrationTestBase
{
    private readonly Faker _faker;

    public CartControllerIntegrationTests()
    {
        _faker = new Faker("pt_BR");
    }

    #region Helper Methods

    /// <summary>
    /// Registra e autentica um novo jogador, retornando o token JWT
    /// </summary>
    private async Task<string> CreateAuthenticatedPlayerAsync()
    {
        var password = "Test@1234";
        var registerDto = new UserCreateRequestDto
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            DisplayName = _faker.Internet.UserName(),
            Email = _faker.Internet.Email(),
            Password = password,
            ConfirmPassword = password,
            Birthday = _faker.Date.Past(30, DateTime.Now.AddYears(-18))
        };

        var response = await ApiClient.PostAsJsonAsync("/api/auth/register-player", registerDto,
            cancellationToken: TestContext.Current.CancellationToken);

        var authResult = await ReadServiceResultAsync<UserAuthResponseDto>(response);
        Assert.NotNull(authResult);
        Assert.NotEmpty(authResult.Token);

        return authResult.Token;
    }

    #endregion

    #region Get Cart Tests

    [Fact]
    public async Task GetCart_DeveRetornarCarrinhoVazio_QuandoNovoUsuario()
    {
        // Arrange
        var token = await CreateAuthenticatedPlayerAsync();
        SetAuthorizationHeader(token);

        // Act
        var response = await ApiClient.GetAsync("/api/cart",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var cart = await ReadServiceResultAsync<CartResponseDto>(response);
        Assert.NotNull(cart);
        Assert.NotNull(cart.Items);
        Assert.Empty(cart.Items);
        Assert.Equal(0, cart.Total);
    }

    [Fact]
    public async Task GetCart_DeveRetornarUnauthorized_QuandoSemAutenticacao()
    {
        // Arrange
        ClearAuthorizationHeader();

        // Act
        var response = await ApiClient.GetAsync("/api/cart",
       cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Add Item Tests

    [Fact]
    public async Task AddItem_DeveAdicionarJogoAoCarrinho()
    {
        // Arrange
        var token = await CreateAuthenticatedPlayerAsync();
        SetAuthorizationHeader(token);

        // Obt�m um jogo dispon�vel da galeria
        var gamesResponse = await ApiClient.GetAsync("/api/gallery?PageNumber=1&PageSize=1",cancellationToken: TestContext.Current.CancellationToken);
        var gamesResult = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(gamesResponse);

        Assert.NotNull(gamesResult);
        Assert.NotEmpty(gamesResult.Items);

        var gameId = gamesResult.Items.First().Id;

        // Act
        var response = await ApiClient.PostAsync($"/api/cart/add/{gameId}", null, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verifica se o item foi adicionado
        var cartResponse = await ApiClient.GetAsync("/api/cart",
           cancellationToken: TestContext.Current.CancellationToken);
        var cart = await ReadServiceResultAsync<CartResponseDto>(cartResponse);

        Assert.NotNull(cart);
        Assert.Single(cart.Items);
        Assert.Contains(cart.Items, item => item.GameId == gameId);
    }

    [Fact]
    public async Task AddItem_DeveRetornarUnauthorized_QuandoSemAutenticacao()
    {
        // Arrange
        ClearAuthorizationHeader();
        var gameId = Guid.NewGuid();

        // Act
        var response = await ApiClient.PostAsync($"/api/cart/add/{gameId}", null, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddItem_DeveRetornarBadRequest_QuandoJogoJaExisteNoCarrinho()
    {
        // Arrange
        var token = await CreateAuthenticatedPlayerAsync();
        SetAuthorizationHeader(token);

        // Obt�m um jogo dispon�vel
        var gamesResponse = await ApiClient.GetAsync("/api/gallery?PageNumber=1&PageSize=1",cancellationToken: TestContext.Current.CancellationToken);
        var gamesResult = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(gamesResponse);
        var gameId = gamesResult.Items.First().Id;

        // Adiciona o jogo pela primeira vez
        await ApiClient.PostAsync($"/api/cart/add/{gameId}", null, cancellationToken: TestContext.Current.CancellationToken);

        // Act - Tenta adicionar novamente
        var response = await ApiClient.PostAsync($"/api/cart/add/{gameId}", null,cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddItem_DeveRetornarBadRequest_QuandoJogoNaoExiste()
    {
        // Arrange
        var token = await CreateAuthenticatedPlayerAsync();
        SetAuthorizationHeader(token);
        var gameIdInexistente = Guid.NewGuid();

        // Act
        var response = await ApiClient.PostAsync($"/api/cart/add/{gameIdInexistente}", null, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Remove Item Tests

    [Fact]
    public async Task RemoveItem_DeveRemoverJogoDoCarrinho()
    {
        // Arrange
        var token = await CreateAuthenticatedPlayerAsync();
        SetAuthorizationHeader(token);

        // Adiciona um jogo ao carrinho
        var gamesResponse = await ApiClient.GetAsync(
        "/api/gallery?PageNumber=1&PageSize=1",
    cancellationToken: TestContext.Current.CancellationToken);
        var gamesResult = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(gamesResponse);
        var gameId = gamesResult.Items.First().Id;

        await ApiClient.PostAsync($"/api/cart/add/{gameId}", null,
    cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var response = await ApiClient.DeleteAsync($"/api/cart/remove/{gameId}",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verifica se o carrinho est� vazio
        var cartResponse = await ApiClient.GetAsync("/api/cart",
            cancellationToken: TestContext.Current.CancellationToken);
        var cart = await ReadServiceResultAsync<CartResponseDto>(cartResponse);

        Assert.NotNull(cart);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public async Task RemoveItem_DeveRetornarUnauthorized_QuandoSemAutenticacao()
    {
        // Arrange
        ClearAuthorizationHeader();
        var gameId = Guid.NewGuid();

        // Act
        var response = await ApiClient.DeleteAsync($"/api/cart/remove/{gameId}",
       cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RemoveItem_DeveRetornarBadRequest_QuandoJogoNaoEstaNoCarrinho()
    {
        // Arrange
        var token = await CreateAuthenticatedPlayerAsync();
        SetAuthorizationHeader(token);
        var gameIdNaoExistente = Guid.NewGuid();

        // Act
        var response = await ApiClient.DeleteAsync($"/api/cart/remove/{gameIdNaoExistente}",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Clear Cart Tests

    [Fact]
    public async Task ClearCart_DeveLimparTodosOsItensDoCarrinho()
    {
        // Arrange
        var token = await CreateAuthenticatedPlayerAsync();
        SetAuthorizationHeader(token);

        // Adiciona m�ltiplos jogos ao carrinho
        var gamesResponse = await ApiClient.GetAsync(
                  "/api/gallery?PageNumber=1&PageSize=3",
                  cancellationToken: TestContext.Current.CancellationToken);
        var gamesResult = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(gamesResponse);

        foreach (var game in gamesResult.Items)
        {
            await ApiClient.PostAsync($"/api/cart/add/{game.Id}", null,
           cancellationToken: TestContext.Current.CancellationToken);
        }

        // Act
        var response = await ApiClient.DeleteAsync("/api/cart/clear",
        cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verifica se o carrinho est� vazio
        var cartResponse = await ApiClient.GetAsync("/api/cart",
            cancellationToken: TestContext.Current.CancellationToken);
        var cart = await ReadServiceResultAsync<CartResponseDto>(cartResponse);

        Assert.NotNull(cart);
        Assert.Empty(cart.Items);
        Assert.Equal(0, cart.Total);
    }

    [Fact]
    public async Task ClearCart_DeveRetornarUnauthorized_QuandoSemAutenticacao()
    {
        // Arrange
        ClearAuthorizationHeader();

        // Act
        var response = await ApiClient.DeleteAsync("/api/cart/clear",
                  cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ClearCart_DeveFuncionarComCarrinhoVazio()
    {
        // Arrange
        var token = await CreateAuthenticatedPlayerAsync();
        SetAuthorizationHeader(token);

        // Act
        var response = await ApiClient.DeleteAsync("/api/cart/clear",
          cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Integration Flow Tests

    [Fact]
    public async Task FluxoCompleto_AdicionarRemoverLimparCarrinho_DeveFuncionarCorretamente()
    {
        // Arrange
        var token = await CreateAuthenticatedPlayerAsync();
        SetAuthorizationHeader(token);

        // Obt�m jogos dispon�veis
        var gamesResponse = await ApiClient.GetAsync(
      "/api/gallery?PageNumber=1&PageSize=3",
     cancellationToken: TestContext.Current.CancellationToken);
        var gamesResult = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(gamesResponse);
        var games = gamesResult.Items.ToList();

        // 1. Adiciona tr�s jogos
        foreach (var game in games)
        {
            var addResponse = await ApiClient.PostAsync($"/api/cart/add/{game.Id}", null,
        cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, addResponse.StatusCode);
        }

        // Verifica se tem 3 itens
        var cartResponse1 = await ApiClient.GetAsync("/api/cart",
            cancellationToken: TestContext.Current.CancellationToken);
        var cart1 = await ReadServiceResultAsync<CartResponseDto>(cartResponse1);
        Assert.Equal(3, cart1.Items.Count());

        // 2. Remove um jogo
        var removeResponse = await ApiClient.DeleteAsync($"/api/cart/remove/{games[0].Id}",
                cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, removeResponse.StatusCode);

        // Verifica se tem 2 itens
        var cartResponse2 = await ApiClient.GetAsync("/api/cart",
    cancellationToken: TestContext.Current.CancellationToken);
        var cart2 = await ReadServiceResultAsync<CartResponseDto>(cartResponse2);
        Assert.Equal(2, cart2.Items.Count());

        // 3. Limpa o carrinho
        var clearResponse = await ApiClient.DeleteAsync("/api/cart/clear",
                  cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, clearResponse.StatusCode);

        // Verifica se est� vazio
        var cartResponse3 = await ApiClient.GetAsync("/api/cart",
          cancellationToken: TestContext.Current.CancellationToken);
        var cart3 = await ReadServiceResultAsync<CartResponseDto>(cartResponse3);
        Assert.Empty(cart3.Items);
    }

    [Fact]
    public async Task FluxoCompleto_CalculoDoTotalDoCarrinho_DeveEstarCorreto()
    {
        // Arrange
        var token = await CreateAuthenticatedPlayerAsync();
        SetAuthorizationHeader(token);

        // Obt�m jogos com pre�os
        var gamesResponse = await ApiClient.GetAsync(
   "/api/gallery?PageNumber=1&PageSize=2",
          cancellationToken: TestContext.Current.CancellationToken);
        var gamesResult = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(gamesResponse);
        var games = gamesResult.Items.ToList();

        decimal totalEsperado = 0;

        // Adiciona jogos e calcula total esperado
        foreach (var game in games)
        {
            await ApiClient.PostAsync($"/api/cart/add/{game.Id}", null,
 cancellationToken: TestContext.Current.CancellationToken);
            totalEsperado += game.FinalPrice;
        }

        // Act
        var cartResponse = await ApiClient.GetAsync("/api/cart",
            cancellationToken: TestContext.Current.CancellationToken);
        var cart = await ReadServiceResultAsync<CartResponseDto>(cartResponse);

        // Assert
        Assert.NotNull(cart);
        Assert.Equal(totalEsperado, cart.Total);
        Assert.Equal(games.Count, cart.Items.Count());
    }

    #endregion
}
