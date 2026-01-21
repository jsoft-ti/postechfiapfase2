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
/// Testes de integra��o para o PurchaseController
/// Testa todos os endpoints de compra e hist�rico de compras
/// </summary>
public class PurchaseControllerIntegrationTests : ApiIntegrationTestBase
{
    private readonly Faker _faker;

    public PurchaseControllerIntegrationTests()
    {
        _faker = new Faker("pt_BR");
    }

    #region Helper Methods

    /// <summary>
/// Registra e autentica um novo jogador, retornando o token e ID do jogador
/// </summary>
    private async Task<(string Token, Guid PlayerId)> CreateAuthenticatedPlayerWithIdAsync()
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

        return (authResult.Token, authResult.User.Id);
    }

    /// <summary>
    /// Obt�m um jogo dispon�vel da galeria
    /// </summary>
    private async Task<GalleryGameResponseDto> GetAvailableGameAsync()
    {
        var response = await ApiClient.GetAsync(
 "/api/gallery?PageNumber=1&PageSize=1",
      cancellationToken: TestContext.Current.CancellationToken);

        var result = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(response);
        Assert.NotEmpty(result.Items);

    return result.Items.First();
  }

    #endregion

    #region Register Single Purchase Tests

    [Fact]
    public async Task RegisterSinglePurchase_DeveRealizarCompraComSucesso()
    {
        // Arrange
     var (token, playerId) = await CreateAuthenticatedPlayerWithIdAsync();
        SetAuthorizationHeader(token);

        var game = await GetAvailableGameAsync();
        var purchaseDto = new PurchaseCreateRequestDto
        {
            PlayerId = playerId,
        GameId = game.Id
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/purchase/single", purchaseDto,
         cancellationToken: TestContext.Current.CancellationToken);

   // Assert
 Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var purchase = await ReadServiceResultAsync<PurchaseResponseDto>(response);
        Assert.NotNull(purchase);
      Assert.Equal(game.Id, purchase.PurchaseId);
    }

    [Fact]
    public async Task RegisterSinglePurchase_DeveRetornarUnauthorized_QuandoSemAutenticacao()
    {
  // Arrange
        ClearAuthorizationHeader();

  var purchaseDto = new PurchaseCreateRequestDto
  {
       PlayerId = Guid.NewGuid(),
          GameId = Guid.NewGuid()
        };

        // Act
     var response = await ApiClient.PostAsJsonAsync("/api/purchase/single", purchaseDto,
   cancellationToken: TestContext.Current.CancellationToken);

      // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegisterSinglePurchase_DeveRetornarBadRequest_QuandoPlayerIdDiferente()
    {
        // Arrange
        var (token, playerId) = await CreateAuthenticatedPlayerWithIdAsync();
    SetAuthorizationHeader(token);

        var game = await GetAvailableGameAsync();
        var purchaseDto = new PurchaseCreateRequestDto
     {
            PlayerId = Guid.NewGuid(), // ID diferente do jogador autenticado
  GameId = game.Id
    };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/purchase/single", purchaseDto,
   cancellationToken: TestContext.Current.CancellationToken);

  // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterSinglePurchase_DeveRetornarBadRequest_QuandoJogoNaoExiste()
    {
        // Arrange
        var (token, playerId) = await CreateAuthenticatedPlayerWithIdAsync();
        SetAuthorizationHeader(token);

        var purchaseDto = new PurchaseCreateRequestDto
    {
       PlayerId = playerId,
            GameId = Guid.NewGuid() // Jogo que n�o existe
        };

      // Act
        var response = await ApiClient.PostAsJsonAsync("/api/purchase/single", purchaseDto,
        cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterSinglePurchase_DeveRetornarBadRequest_QuandoJogoJaPossuido()
    {
        // Arrange
        var (token, playerId) = await CreateAuthenticatedPlayerWithIdAsync();
      SetAuthorizationHeader(token);

        var game = await GetAvailableGameAsync();
 var purchaseDto = new PurchaseCreateRequestDto
        {
        PlayerId = playerId,
  GameId = game.Id
        };

        // Primeira compra
        await ApiClient.PostAsJsonAsync("/api/purchase/single", purchaseDto,
  cancellationToken: TestContext.Current.CancellationToken);

        // Act - Tenta comprar novamente
        var response = await ApiClient.PostAsJsonAsync("/api/purchase/single", purchaseDto,
     cancellationToken: TestContext.Current.CancellationToken);

 // Assert
  Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Register Purchase From Cart Tests

    [Fact]
    public async Task RegisterPurchaseFromCart_DeveComprarItensDoCarrinho()
    {
        // Arrange
        var (token, playerId) = await CreateAuthenticatedPlayerWithIdAsync();
        SetAuthorizationHeader(token);

        // Adiciona jogos ao carrinho
        var gamesResponse = await ApiClient.GetAsync(
   "/api/gallery?PageNumber=1&PageSize=2",
    cancellationToken: TestContext.Current.CancellationToken);
        var gamesResult = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(gamesResponse);

        foreach (var game in gamesResult.Items)
        {
  await ApiClient.PostAsync($"/api/cart/add/{game.Id}", null,
       cancellationToken: TestContext.Current.CancellationToken);
        }

    // Act
      var response = await ApiClient.PostAsync("/api/purchase/cart", null,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verifica se o carrinho foi limpo
        var cartResponse = await ApiClient.GetAsync("/api/cart",
            cancellationToken: TestContext.Current.CancellationToken);
        var cart = await ReadServiceResultAsync<CartResponseDto>(cartResponse);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public async Task RegisterPurchaseFromCart_DeveRetornarUnauthorized_QuandoSemAutenticacao()
 {
      // Arrange
        ClearAuthorizationHeader();

     // Act
        var response = await ApiClient.PostAsync("/api/purchase/cart", null,
       cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegisterPurchaseFromCart_DeveRetornarBadRequest_QuandoCarrinhoVazio()
  {
        // Arrange
        var (token, playerId) = await CreateAuthenticatedPlayerWithIdAsync();
   SetAuthorizationHeader(token);

        // Act
        var response = await ApiClient.PostAsync("/api/purchase/cart", null,
     cancellationToken: TestContext.Current.CancellationToken);

        // Assert
  Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Get Purchases By Player Tests

    [Fact]
    public async Task GetPurchasesByPlayer_DeveRetornarHistoricoDeCompras()
    {
    // Arrange
   var (token, playerId) = await CreateAuthenticatedPlayerWithIdAsync();
        SetAuthorizationHeader(token);

        // Realiza uma compra
var game = await GetAvailableGameAsync();
 var purchaseDto = new PurchaseCreateRequestDto
        {
 PlayerId = playerId,
      GameId = game.Id
        };

        await ApiClient.PostAsJsonAsync("/api/purchase/single", purchaseDto,
   cancellationToken: TestContext.Current.CancellationToken);

        // Act
   var response = await ApiClient.GetAsync(
            "/api/purchase/player?PageNumber=1&PageSize=10",
        cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

  var result = await ReadServiceResultAsync<PagedResult<PurchaseResponseDto>>(response);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
        Assert.Contains(result.Items, p => p.GameName == game.Title);
    }

    [Fact]
    public async Task GetPurchasesByPlayer_DeveRetornarUnauthorized_QuandoSemAutenticacao()
    {
        // Arrange
        ClearAuthorizationHeader();

  // Act
        var response = await ApiClient.GetAsync(
        "/api/purchase/player?PageNumber=1&PageSize=10",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetPurchasesByPlayer_DeveRetornarListaVazia_QuandoSemCompras()
    {
      // Arrange
        var (token, playerId) = await CreateAuthenticatedPlayerWithIdAsync();
        SetAuthorizationHeader(token);

   // Act
        var response = await ApiClient.GetAsync(
    "/api/purchase/player?PageNumber=1&PageSize=10",
          cancellationToken: TestContext.Current.CancellationToken);

        // Assert
Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ReadServiceResultAsync<PagedResult<PurchaseResponseDto>>(response);
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetPurchasesByPlayer_DeveFiltrarPorNomeDoJogo()
  {
        // Arrange
        var (token, playerId) = await CreateAuthenticatedPlayerWithIdAsync();
        SetAuthorizationHeader(token);

        // Realiza uma compra
        var game = await GetAvailableGameAsync();
        var purchaseDto = new PurchaseCreateRequestDto
        {
            PlayerId = playerId,
            GameId = game.Id
        };

        await ApiClient.PostAsJsonAsync("/api/purchase/single", purchaseDto,
            cancellationToken: TestContext.Current.CancellationToken);

   // Act
   var response = await ApiClient.GetAsync(
       $"/api/purchase/player?PageNumber=1&PageSize=10&Filter.GameName={game.Title}",
  cancellationToken: TestContext.Current.CancellationToken);

        // Assert
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ReadServiceResultAsync<PagedResult<PurchaseResponseDto>>(response);
     Assert.NotNull(result);
        Assert.All(result.Items, p => Assert.Contains(game.Title, p.GameName, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetPurchasesByPlayer_DeveFiltrarPorFaixaDePreco()
    {
  // Arrange
        var (token, playerId) = await CreateAuthenticatedPlayerWithIdAsync();
        SetAuthorizationHeader(token);

 // Realiza uma compra
        var game = await GetAvailableGameAsync();
        var purchaseDto = new PurchaseCreateRequestDto
        {
  PlayerId = playerId,
            GameId = game.Id
        };

        await ApiClient.PostAsJsonAsync("/api/purchase/single", purchaseDto,
   cancellationToken: TestContext.Current.CancellationToken);

        decimal minPrice = game.FinalPrice - 10;
    decimal maxPrice = game.FinalPrice + 10;

        // Act
        var response = await ApiClient.GetAsync(
       $"/api/purchase/player?PageNumber=1&PageSize=10&Filter.MinPrice={minPrice}&Filter.MaxPrice={maxPrice}",
      cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ReadServiceResultAsync<PagedResult<PurchaseResponseDto>>(response);
        Assert.NotNull(result);
        Assert.All(result.Items, p =>
     {
     Assert.True(p.Price >= minPrice);
     Assert.True(p.Price <= maxPrice);
  });
    }

    [Fact]
    public async Task GetPurchasesByPlayer_DeveOrdenarPorData()
    {
     // Arrange
        var (token, playerId) = await CreateAuthenticatedPlayerWithIdAsync();
        SetAuthorizationHeader(token);

  // Realiza m�ltiplas compras
        var gamesResponse = await ApiClient.GetAsync(
    "/api/gallery?PageNumber=1&PageSize=2",
   cancellationToken: TestContext.Current.CancellationToken);
 var gamesResult = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(gamesResponse);

        foreach (var game in gamesResult.Items)
        {
          var purchaseDto = new PurchaseCreateRequestDto
            {
                PlayerId = playerId,
    GameId = game.Id
       };

     await ApiClient.PostAsJsonAsync("/api/purchase/single", purchaseDto,
                cancellationToken: TestContext.Current.CancellationToken);
        }

        // Act
   var response = await ApiClient.GetAsync(
         "/api/purchase/player?PageNumber=1&PageSize=10&OrderBy.SortBy=Date&OrderBy.Ascending=false",
     cancellationToken: TestContext.Current.CancellationToken);

      // Assert
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ReadServiceResultAsync<PagedResult<PurchaseResponseDto>>(response);
        Assert.NotNull(result);

        if (result.Items.Count() > 1)
        {
    var purchases = result.Items.ToList();
            for (int i = 0; i < purchases.Count - 1; i++)
  {
        Assert.True(purchases[i].PurchaseDate >= purchases[i + 1].PurchaseDate);
    }
        }
    }

    [Fact]
    public async Task GetPurchasesByPlayer_DeveRetornarBadRequest_QuandoPageNumberInvalido()
    {
        // Arrange
        var (token, playerId) = await CreateAuthenticatedPlayerWithIdAsync();
        SetAuthorizationHeader(token);

  // Act
        var response = await ApiClient.GetAsync(
       "/api/purchase/player?PageNumber=0&PageSize=10",
    cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Integration Flow Tests

    [Fact]
    public async Task FluxoCompleto_ComprarJogoEVerificarHistorico_DeveFuncionarCorretamente()
    {
        // Arrange
        var (token, playerId) = await CreateAuthenticatedPlayerWithIdAsync();
  SetAuthorizationHeader(token);

        // 1. Obt�m jogo dispon�vel
        var game = await GetAvailableGameAsync();

     // 2. Realiza compra
   var purchaseDto = new PurchaseCreateRequestDto
  {
        PlayerId = playerId,
   GameId = game.Id
        };

        var purchaseResponse = await ApiClient.PostAsJsonAsync("/api/purchase/single", purchaseDto,
            cancellationToken: TestContext.Current.CancellationToken);
      Assert.Equal(HttpStatusCode.OK, purchaseResponse.StatusCode);

        // 3. Verifica hist�rico
        var historyResponse = await ApiClient.GetAsync(
      "/api/purchase/player?PageNumber=1&PageSize=10",
        cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, historyResponse.StatusCode);

        var history = await ReadServiceResultAsync<PagedResult<PurchaseResponseDto>>(historyResponse);
      Assert.NotEmpty(history.Items);
     Assert.Contains(history.Items, p => p.GameName == game.Title);

        // 4. Verifica se o jogo est� na biblioteca
 var libraryResponse = await ApiClient.GetAsync(
            "/api/player/library?PageNumber=1&PageSize=10",
       cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, libraryResponse.StatusCode);

        var library = await ReadServiceResultAsync<PagedResult<LibraryGameResponseDto>>(libraryResponse);
   Assert.Contains(library.Items, g => g.Title == game.Title);
    }

    [Fact]
    public async Task FluxoCompleto_ComprarCarrinhoCompleto_DeveFuncionarCorretamente()
    {
        // Arrange
        var (token, playerId) = await CreateAuthenticatedPlayerWithIdAsync();
        SetAuthorizationHeader(token);

        // 1. Adiciona jogos ao carrinho
        var gamesResponse = await ApiClient.GetAsync(
            "/api/gallery?PageNumber=1&PageSize=3",
         cancellationToken: TestContext.Current.CancellationToken);
        var gamesResult = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(gamesResponse);
        var games = gamesResult.Items.ToList();

     foreach (var game in games)
        {
    await ApiClient.PostAsync($"/api/cart/add/{game.Id}", null,
     cancellationToken: TestContext.Current.CancellationToken);
        }

        // 2. Verifica carrinho antes da compra
        var cartBeforeResponse = await ApiClient.GetAsync("/api/cart",
      cancellationToken: TestContext.Current.CancellationToken);
        var cartBefore = await ReadServiceResultAsync<CartResponseDto>(cartBeforeResponse);
        Assert.Equal(games.Count, cartBefore.Items.Count());

  // 3. Realiza compra do carrinho
   var purchaseResponse = await ApiClient.PostAsync("/api/purchase/cart", null,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, purchaseResponse.StatusCode);

        // 4. Verifica se o carrinho foi esvaziado
        var cartAfterResponse = await ApiClient.GetAsync("/api/cart",
      cancellationToken: TestContext.Current.CancellationToken);
        var cartAfter = await ReadServiceResultAsync<CartResponseDto>(cartAfterResponse);
Assert.Empty(cartAfter.Items);

   // 5. Verifica hist�rico de compras
     var historyResponse = await ApiClient.GetAsync(
       "/api/purchase/player?PageNumber=1&PageSize=10",
            cancellationToken: TestContext.Current.CancellationToken);
   var history = await ReadServiceResultAsync<PagedResult<PurchaseResponseDto>>(historyResponse);
        Assert.True(history.Items.Count() >= games.Count);

        // 6. Verifica se todos os jogos est�o na biblioteca
        var libraryResponse = await ApiClient.GetAsync(
            "/api/player/library?PageNumber=1&PageSize=10",
            cancellationToken: TestContext.Current.CancellationToken);
     var library = await ReadServiceResultAsync<PagedResult<LibraryGameResponseDto>>(libraryResponse);
        
 foreach (var game in games)
        {
   Assert.Contains(library.Items, g => g.Title == game.Title);
        }
    }

    [Fact]
    public async Task FluxoCompleto_MultiplasComprasEFiltros_DeveFuncionarCorretamente()
    {
        // Arrange
 var (token, playerId) = await CreateAuthenticatedPlayerWithIdAsync();
        SetAuthorizationHeader(token);

        // 1. Realiza m�ltiplas compras
        var gamesResponse = await ApiClient.GetAsync(
          "/api/gallery?PageNumber=1&PageSize=3",
cancellationToken: TestContext.Current.CancellationToken);
 var gamesResult = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(gamesResponse);

      foreach (var game in gamesResult.Items)
        {
            var purchaseDto = new PurchaseCreateRequestDto
            {
       PlayerId = playerId,
        GameId = game.Id
      };

 await ApiClient.PostAsJsonAsync("/api/purchase/single", purchaseDto,
                cancellationToken: TestContext.Current.CancellationToken);
        }

        // 2. Lista todas as compras
        var allPurchasesResponse = await ApiClient.GetAsync(
            "/api/purchase/player?PageNumber=1&PageSize=10",
            cancellationToken: TestContext.Current.CancellationToken);
    var allPurchases = await ReadServiceResultAsync<PagedResult<PurchaseResponseDto>>(allPurchasesResponse);
        Assert.True(allPurchases.Items.Count() >= 3);

     // 3. Filtra por nome de jogo espec�fico
   var firstGame = gamesResult.Items.First();
      var filteredResponse = await ApiClient.GetAsync(
     $"/api/purchase/player?PageNumber=1&PageSize=10&Filter.GameName={firstGame.Title}",
            cancellationToken: TestContext.Current.CancellationToken);
   var filteredPurchases = await ReadServiceResultAsync<PagedResult<PurchaseResponseDto>>(filteredResponse);
  Assert.All(filteredPurchases.Items, p => Assert.Contains(firstGame.Title, p.GameName));

        // 4. Ordena por pre�o
        var orderedResponse = await ApiClient.GetAsync(
 "/api/purchase/player?PageNumber=1&PageSize=10&OrderBy.SortBy=Price&OrderBy.Ascending=true",
            cancellationToken: TestContext.Current.CancellationToken);
 var orderedPurchases = await ReadServiceResultAsync<PagedResult<PurchaseResponseDto>>(orderedResponse);
        
 if (orderedPurchases.Items.Count() > 1)
      {
            var purchases = orderedPurchases.Items.ToList();
       for (int i = 0; i < purchases.Count - 1; i++)
 {
 Assert.True(purchases[i].Price <= purchases[i + 1].Price);
            }
        }
    }

    #endregion
}
