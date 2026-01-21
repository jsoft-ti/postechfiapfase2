using Bogus;
using Application.Dto.Request;
using Application.Dto.Response;
using Tests.Integration.Base;
using Tests.Integration.Base.Model.Results;
using System.Net.Http.Json;

namespace Tests.Integration;

/// <summary>
/// Testes de integração para o AuthController
/// Testa todos os endpoints de autenticação e autorização
/// </summary>
public class AuthControllerIntegrationTests : ApiIntegrationTestBase
{
    private readonly Faker _faker;

    public AuthControllerIntegrationTests()
    {
        _faker = new Faker("pt_BR");
    }

    #region Register Player Tests

    [Fact]
    public async Task RegisterPlayer_DeveRegistrarNovoUsuarioComSucesso()
    {
        // Arrange
        var dto = new UserCreateRequestDto
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            DisplayName = _faker.Internet.UserName(),
            Email = _faker.Internet.Email(),
            Password = "Test@1234",
            ConfirmPassword = "Test@1234",
            Birthday = _faker.Date.Past(30, DateTime.Now.AddYears(-18))
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/auth/register-player", dto, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // ✅ Usa o helper method para extrair dados do ServiceResult
        var result = await ReadServiceResultAsync<UserAuthResponseDto>(response);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.NotEmpty(result.RefreshToken);
        Assert.NotNull(result.User);
        Assert.Equal(dto.Email, result.User.Email);
        Assert.Equal(dto.DisplayName, result.User.DisplayName);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task RegisterPlayer_DeveRetornarBadRequest_QuandoEmailDuplicado()
    {
        // Arrange - Registra primeiro usuário
        var email = _faker.Internet.Email();
        var firstUser = new UserCreateRequestDto
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            DisplayName = _faker.Internet.UserName(),
            Email = email,
            Password = "Test@1234",
            ConfirmPassword = "Test@1234",
            Birthday = _faker.Date.Past(30, DateTime.Now.AddYears(-18))
        };

        await ApiClient.PostAsJsonAsync("/api/auth/register-player", firstUser, cancellationToken: TestContext.Current.CancellationToken);

        // Act - Tenta registrar com mesmo email
        var duplicateUser = new UserCreateRequestDto
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            DisplayName = _faker.Internet.UserName(),
            Email = email, // Mesmo email
            Password = "Test@1234",
            ConfirmPassword = "Test@1234",
            Birthday = _faker.Date.Past(30, DateTime.Now.AddYears(-18))
        };

        var response = await ApiClient.PostAsJsonAsync("/api/auth/register-player", duplicateUser, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterPlayer_DeveRetornarBadRequest_QuandoSenhasNaoConferem()
    {
        // Arrange
        var dto = new UserCreateRequestDto
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            DisplayName = _faker.Internet.UserName(),
            Email = _faker.Internet.Email(),
            Password = "Test@1234",
            ConfirmPassword = "Different@1234", // Senha diferente
            Birthday = _faker.Date.Past(30, DateTime.Now.AddYears(-18))
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/auth/register-player", dto, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterPlayer_DeveRetornarBadRequest_QuandoSenhaFraca()
    {
        // Arrange
        var dto = new UserCreateRequestDto
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            DisplayName = _faker.Internet.UserName(),
            Email = _faker.Internet.Email(),
            Password = "123", // Senha fraca
            ConfirmPassword = "123",
            Birthday = _faker.Date.Past(30, DateTime.Now.AddYears(-18))
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/auth/register-player", dto, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterPlayer_DeveRetornarBadRequest_QuandoEmailInvalido()
    {
        // Arrange
        var dto = new UserCreateRequestDto
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            DisplayName = _faker.Internet.UserName(),
            Email = "email-invalido", // Email inválido
            Password = "Test@1234",
            ConfirmPassword = "Test@1234",
            Birthday = _faker.Date.Past(30, DateTime.Now.AddYears(-18))
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/auth/register-player", dto, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterPlayer_DeveRetornarBadRequest_QuandoNomeMuitoCurto()
    {
        // Arrange
        var dto = new UserCreateRequestDto
        {
            FirstName = "A", // Nome muito curto
            LastName = "B",
            DisplayName = "AB",
            Email = _faker.Internet.Email(),
            Password = "Test@1234",
            ConfirmPassword = "Test@1234",
            Birthday = _faker.Date.Past(30, DateTime.Now.AddYears(-18))
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/auth/register-player", dto, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_DeveAutenticarUsuarioComSucesso()
    {
        // Arrange - Primeiro registra o usuário
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

        await ApiClient.PostAsJsonAsync("/api/auth/register-player", registerDto, cancellationToken: TestContext.Current.CancellationToken);

        // Act - Faz login
        var loginDto = new UserLoginRequestDto
        {
            Email = registerDto.Email,
            Password = password
        };

        var response = await ApiClient.PostAsJsonAsync("/api/auth/login", loginDto, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // ✅ Usa o helper method
        var result = await ReadServiceResultAsync<UserAuthResponseDto>(response);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.NotEmpty(result.RefreshToken);
        Assert.NotNull(result.User);
        Assert.Equal(registerDto.Email, result.User.Email);
    }

    [Fact]
    public async Task Login_DeveRetornarUnauthorized_QuandoCredenciaisInvalidas()
    {
        // Arrange
        var dto = new UserLoginRequestDto
        {
            Email = "usuario.inexistente@teste.com",
            Password = "SenhaErrada@123"
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/auth/login", dto, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_DeveRetornarUnauthorized_QuandoSenhaIncorreta()
    {
        // Arrange - Registra usuário
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

        await ApiClient.PostAsJsonAsync("/api/auth/register-player", registerDto, cancellationToken: TestContext.Current.CancellationToken);

        // Act - Tenta login com senha errada
        var loginDto = new UserLoginRequestDto
        {
            Email = registerDto.Email,
            Password = "SenhaErrada@123"
        };

        var response = await ApiClient.PostAsJsonAsync("/api/auth/login", loginDto, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public async Task RefreshToken_DeveGerarNovoTokenComSucesso()
    {
        // Arrange - Registra usuário e obtém tokens
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

        var registerResponse = await ApiClient.PostAsJsonAsync("/api/auth/register-player", registerDto, cancellationToken: TestContext.Current.CancellationToken);

        // ✅ Usa o helper method
        var initialTokens = await ReadServiceResultAsync<UserAuthResponseDto>(registerResponse);
        Assert.NotNull(initialTokens);

        // Act - Usa refresh token
        var refreshDto = new RefreshTokenRequestDto
        {
            Token = initialTokens.Token,
            RefreshToken = initialTokens.RefreshToken
        };

        var response = await ApiClient.PostAsJsonAsync("/api/auth/refresh", refreshDto, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // ✅ Usa o helper method
        var newTokens = await ReadServiceResultAsync<UserAuthResponseDto>(response);
        Assert.NotNull(newTokens);
        Assert.NotEmpty(newTokens.Token);
        Assert.NotEmpty(newTokens.RefreshToken);
        Assert.NotEqual(initialTokens.Token, newTokens.Token); // Token deve ser diferente
    }

    [Fact]
    public async Task RefreshToken_DeveRetornarUnauthorized_QuandoRefreshTokenInvalido()
    {
        // Arrange
        var dto = new RefreshTokenRequestDto
        {
            Token = "token.invalido.aqui",
            RefreshToken = "refresh-token-invalido"
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/auth/refresh", dto, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Get Current User Tests

    [Fact]
    public async Task GetCurrentUser_DeveRetornarDadosDoUsuarioAutenticado()
    {
        // Arrange - Registra e obtém token
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

        var registerResponse = await ApiClient.PostAsJsonAsync("/api/auth/register-player", registerDto, cancellationToken: TestContext.Current.CancellationToken);

        // ✅ Usa o helper method
        var authResult = await ReadServiceResultAsync<UserAuthResponseDto>(registerResponse);
        Assert.NotNull(authResult);

        SetAuthorizationHeader(authResult.Token);

        // Act
        var response = await ApiClient.GetAsync("/api/auth/me", cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // ✅ Usa o helper method
        var user = await ReadServiceResultAsync<UserInfoResponseDto>(response);
        Assert.NotNull(user);
        Assert.Equal(registerDto.Email, user.Email);
        Assert.Equal(registerDto.DisplayName, user.DisplayName);
        Assert.Equal(registerDto.FirstName, user.FirstName);
        Assert.Equal(registerDto.LastName, user.LastName);
    }

    [Fact]
    public async Task GetCurrentUser_DeveRetornarUnauthorized_QuandoSemToken()
    {
        // Arrange
        ClearAuthorizationHeader();

        // Act
        var response = await ApiClient.GetAsync("/api/auth/me", cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_DeveRetornarUnauthorized_QuandoTokenInvalido()
    {
        // Arrange
        SetAuthorizationHeader("token.invalido.aqui");

        // Act
        var response = await ApiClient.GetAsync("/api/auth/me", cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_DeveRetornarUnauthorized_QuandoTokenExpirado()
    {
        // Arrange - Token expirado (simulado com token inválido)
        SetAuthorizationHeader("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE1MTYyMzkwMjJ9.invalid");

        // Act
        var response = await ApiClient.GetAsync("/api/auth/me", cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Integration Flow Tests

    // ✅ Teste simplificado para verificar autenticação básica
    [Fact]
    public async Task GetCurrentUser_SimpleTest_DeveFuncionar()
    {
        // Arrange
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

        // 1. Registro
        var registerResponse = await ApiClient.PostAsJsonAsync("/api/auth/register-player", registerDto, cancellationToken: TestContext.Current.CancellationToken);
        var registerTokens = await ReadServiceResultAsync<UserAuthResponseDto>(registerResponse);

        // 2. Configura header de autorização
        SetAuthorizationHeader(registerTokens.Token);
        var getUserResponse = await ApiClient.GetAsync("/api/Auth/me", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, getUserResponse.StatusCode);
    }

    [Fact]
    public async Task FluxoCompleto_RegistroLoginRefreshGetUser_DeveFuncionarCorretamente()
    {
        // Arrange
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

        // 1. Registro
        var registerResponse = await ApiClient.PostAsJsonAsync("/api/auth/register-player", registerDto, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        // ✅ Usa o helper method
        var registerTokens = await ReadServiceResultAsync<UserAuthResponseDto>(registerResponse);
        Assert.NotNull(registerTokens);

        // 2. Login
        var loginDto = new UserLoginRequestDto
        {
            Email = registerDto.Email,
            Password = password
        };
        var loginResponse = await ApiClient.PostAsJsonAsync("/api/auth/login", loginDto, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // ✅ Usa o helper method
        var loginTokens = await ReadServiceResultAsync<UserAuthResponseDto>(loginResponse);
        Assert.NotNull(loginTokens);

        // 3. Get Current User com token de login
        SetAuthorizationHeader(loginTokens.Token);
        // ✅ Usa GetWithAuthAsync para enviar o header explicitamente
        var getUserResponse1 = await GetWithAuthAsync("/api/Auth/me", TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.OK, getUserResponse1.StatusCode);
        var user1 = await ReadServiceResultAsync<UserInfoResponseDto>(getUserResponse1);

        // ✅ Usa o helper method
        Assert.NotNull(user1);
        Assert.Equal(registerDto.Email, user1.Email);

        // 4. Refresh Token
        var refreshDto = new RefreshTokenRequestDto
        {
            Token = loginTokens.Token,
            RefreshToken = loginTokens.RefreshToken
        };
        var refreshResponse = await ApiClient.PostAsJsonAsync("/api/Auth/refresh", refreshDto, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        // ✅ Usa o helper method
        var newTokens = await ReadServiceResultAsync<UserAuthResponseDto>(refreshResponse);
        Assert.NotNull(newTokens);
        Assert.NotEqual(loginTokens.Token, newTokens.Token);

        // 5. Get Current User com novo token
        SetAuthorizationHeader(newTokens.Token);
        // ✅ Usa GetWithAuthAsync para enviar o header explicitamente
        var getUserResponse2 = await GetWithAuthAsync("/api/Auth/me", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, getUserResponse2.StatusCode);

        // ✅ Usa o helper method
        var user2 = await ReadServiceResultAsync<UserInfoResponseDto>(getUserResponse2);
        Assert.NotNull(user2);
        Assert.Equal(registerDto.Email, user2.Email);
        Assert.Equal(user1.Id, user2.Id); // Mesmo usuário
    }

    #endregion
}
