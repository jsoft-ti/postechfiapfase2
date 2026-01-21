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
/// Testes de integra��o para o GalleryController
/// Testa todos os endpoints p�blicos da galeria de jogos
/// </summary>
public class GalleryControllerIntegrationTests : ApiIntegrationTestBase
{
    private readonly Faker _faker;

    public GalleryControllerIntegrationTests()
    {
        _faker = new Faker("pt_BR");
    }

    #region Get Gallery Games Tests

    [Fact]
    public async Task GetGalleryGames_DeveRetornarListaDePaginada()
    {
        // Arrange
        var request = new PagedRequestDto<GameFilterDto, GameOrderDto>
        {
        PageNumber = 1,
       PageSize = 10
  };

        // Act
        var response = await ApiClient.GetAsync(
            $"/api/gallery?PageNumber={request.PageNumber}&PageSize={request.PageSize}",
     cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var result = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(response);
        Assert.NotNull(result);
   Assert.NotNull(result.Items);
     Assert.True(result.PageNumber > 0);
        Assert.True(result.PageSize > 0);
    }

    [Fact]
    public async Task GetGalleryGames_DeveRetornarBadRequest_QuandoPageNumberInvalido()
    {
// Arrange & Act
        var response = await ApiClient.GetAsync(
            "/api/gallery?PageNumber=0&PageSize=10",
cancellationToken: TestContext.Current.CancellationToken);

        // Assert
Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetGalleryGames_DeveRetornarBadRequest_QuandoPageSizeInvalido()
    {
        // Arrange & Act
        var response = await ApiClient.GetAsync(
   "/api/gallery?PageNumber=1&PageSize=0",
        cancellationToken: TestContext.Current.CancellationToken);

  // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetGalleryGames_DeveFiltrarPorNome()
    {
        // Arrange
        var gameName = "Test";

        // Act
   var response = await ApiClient.GetAsync(
         $"/api/gallery?PageNumber=1&PageSize=10&Filter.Name={gameName}",
  cancellationToken: TestContext.Current.CancellationToken);

  // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(response);
     Assert.NotNull(result);
    }

    [Fact]
public async Task GetGalleryGames_DeveFiltrarPorGenero()
    {
        // Arrange
     var genre = "Action";

    // Act
        var response = await ApiClient.GetAsync(
            $"/api/gallery?PageNumber=1&PageSize=10&Filter.Genre={genre}",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);

   var result = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(response);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetGalleryGames_DeveFiltrarPorFaixaDePreco()
    {
        // Arrange
        decimal minPrice = 10;
   decimal maxPrice = 100;

     // Act
        var response = await ApiClient.GetAsync(
 $"/api/gallery?PageNumber=1&PageSize=10&Filter.MinPrice={minPrice}&Filter.MaxPrice={maxPrice}",
    cancellationToken: TestContext.Current.CancellationToken);

     // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(response);
        Assert.NotNull(result);
        
        // Verifica se os jogos retornados est�o na faixa de pre�o
     foreach (var game in result.Items)
        {
            Assert.True(game.FinalPrice >= minPrice);
       Assert.True(game.FinalPrice <= maxPrice);
    }
    }

    [Fact]
    public async Task GetGalleryGames_DeveOrdenarPorNomeAscendente()
    {
  // Arrange & Act
        var response = await ApiClient.GetAsync(
            "/api/gallery?PageNumber=1&PageSize=10&OrderBy.OrderBy=Name&OrderBy.Ascending=true",
         cancellationToken: TestContext.Current.CancellationToken);

    // Assert
 Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(response);
        Assert.NotNull(result);
        
        if (result.Items.Count() > 1)
        {
        var games = result.Items.ToList();
    for (int i = 0; i < games.Count - 1; i++)
         {
     Assert.True(string.Compare(games[i].Title, games[i + 1].Title, StringComparison.Ordinal) <= 0);
      }
        }
    }

    [Fact]
    public async Task GetGalleryGames_DeveOrdenarPorPrecoDescendente()
    {
      // Arrange & Act
   var response = await ApiClient.GetAsync(
      "/api/gallery?PageNumber=1&PageSize=10&OrderBy.OrderBy=Price&OrderBy.Ascending=false",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

  var result = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(response);
        Assert.NotNull(result);
        
        if (result.Items.Count() > 1)
   {
 var games = result.Items.ToList();
    for (int i = 0; i < games.Count - 1; i++)
   {
          Assert.True(games[i].FinalPrice >= games[i + 1].FinalPrice);
       }
      }
 }

    #endregion

    #region Get Gallery Game By Id Tests

    [Fact]
    public async Task GetGalleryGame_DeveRetornarJogoPorId()
 {
        // Arrange - Primeiro obt�m um jogo da lista
    var listResponse = await ApiClient.GetAsync(
            "/api/gallery?PageNumber=1&PageSize=1",
      cancellationToken: TestContext.Current.CancellationToken);
        var listResult = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(listResponse);
 
        Assert.NotEmpty(listResult.Items);
        var gameId = listResult.Items.First().Id;

        // Act
        var response = await ApiClient.GetAsync(
          $"/api/gallery/{gameId}",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
 Assert.Equal(HttpStatusCode.OK, response.StatusCode);

 var game = await ReadServiceResultAsync<GalleryGameResponseDto>(response);
      Assert.NotNull(game);
     Assert.Equal(gameId, game.Id);
        Assert.NotEmpty(game.Title);
        Assert.True(game.Price >= 0);
  Assert.True(game.FinalPrice >= 0);
    }

    [Fact]
  public async Task GetGalleryGame_DeveRetornarNotFound_QuandoIdNaoExiste()
    {
        // Arrange
  var gameIdInexistente = Guid.NewGuid();

        // Act
        var response = await ApiClient.GetAsync(
$"/api/gallery/{gameIdInexistente}",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
     Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

  [Fact]
    public async Task GetGalleryGame_DeveRetornarInformacoesCompletas()
    {
   // Arrange
var listResponse = await ApiClient.GetAsync(
       "/api/gallery?PageNumber=1&PageSize=1",
          cancellationToken: TestContext.Current.CancellationToken);
        var listResult = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(listResponse);
        var gameId = listResult.Items.First().Id;

     // Act
     var response = await ApiClient.GetAsync(
            $"/api/gallery/{gameId}",
        cancellationToken: TestContext.Current.CancellationToken);

     // Assert
        var game = await ReadServiceResultAsync<GalleryGameResponseDto>(response);
        
        Assert.NotNull(game);
  Assert.NotEmpty(game.Title);
        Assert.NotEmpty(game.Genre);
        Assert.NotEmpty(game.EAN);
        Assert.True(game.Price > 0);
        Assert.True(game.FinalPrice > 0);
    }

    #endregion

    #region Get Promotional Games Tests

    [Fact]
    public async Task GetPromotionalGames_DeveRetornarJogosEmPromocao()
{
      // Arrange & Act
        var response = await ApiClient.GetAsync(
  "/api/gallery/promotions",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var games = await ReadServiceResultAsync<IEnumerable<GalleryGameResponseDto>>(response);
        Assert.NotNull(games);
        
        // Todos os jogos retornados devem estar em promo��o
    foreach (var game in games)
        {
    Assert.True(game.OnSale);
            Assert.True(game.FinalPrice < game.Price);
   }
    }

    [Fact]
    public async Task GetPromotionalGames_DeveFuncionarQuandoNaoHouverPromocoes()
    {
      // Arrange & Act
      var response = await ApiClient.GetAsync(
 "/api/gallery/promotions",
    cancellationToken: TestContext.Current.CancellationToken);

        // Assert
   Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var games = await ReadServiceResultAsync<IEnumerable<GalleryGameResponseDto>>(response);
        Assert.NotNull(games);
    }

    [Fact]
    public async Task GetPromotionalGames_DeveRetornarApenasJogosComDesconto()
    {
        // Arrange & Act
        var response = await ApiClient.GetAsync(
       "/api/gallery/promotions",
   cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var games = await ReadServiceResultAsync<IEnumerable<GalleryGameResponseDto>>(response);
      
        if (games.Any())
        {
            foreach (var game in games)
        {
                Assert.NotNull(game.PromotionDescription);
 Assert.NotEmpty(game.TypePromotion);
           Assert.True(game.PromotionValue > 0);
            }
      }
    }

    #endregion

    #region Integration Flow Tests

    [Fact]
    public async Task FluxoCompleto_ListarFiltrarObterDetalhes_DeveFuncionarCorretamente()
    {
        // 1. Lista todos os jogos
     var listResponse = await ApiClient.GetAsync(
            "/api/gallery?PageNumber=1&PageSize=10",
       cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var listResult = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(listResponse);
        Assert.NotNull(listResult);
        Assert.NotEmpty(listResult.Items);

        // 2. Filtra por g�nero
        var firstGame = listResult.Items.First();
var filterResponse = await ApiClient.GetAsync(
            $"/api/gallery?PageNumber=1&PageSize=10&Filter.Genre={firstGame.Genre}",
         cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, filterResponse.StatusCode);

var filterResult = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(filterResponse);
        Assert.NotNull(filterResult);

     // 3. Obt�m detalhes de um jogo espec�fico
        var detailsResponse = await ApiClient.GetAsync(
        $"/api/gallery/{firstGame.Id}",
        cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, detailsResponse.StatusCode);

  var game = await ReadServiceResultAsync<GalleryGameResponseDto>(detailsResponse);
 Assert.NotNull(game);
        Assert.Equal(firstGame.Id, game.Id);
 Assert.Equal(firstGame.Title, game.Title);
    }

    [Fact]
    public async Task FluxoCompleto_BuscarPromocoesEVerificarDescontos_DeveFuncionarCorretamente()
    {
// 1. Busca jogos em promo��o
        var promoResponse = await ApiClient.GetAsync(
  "/api/gallery/promotions",
      cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, promoResponse.StatusCode);

        var promoGames = await ReadServiceResultAsync<IEnumerable<GalleryGameResponseDto>>(promoResponse);
        Assert.NotNull(promoGames);

        if (promoGames.Any())
        {
  var firstPromo = promoGames.First();

        // 2. Verifica detalhes do jogo em promo��o
          var detailsResponse = await ApiClient.GetAsync(
     $"/api/gallery/{firstPromo.Id}",
 cancellationToken: TestContext.Current.CancellationToken);
   var game = await ReadServiceResultAsync<GalleryGameResponseDto>(detailsResponse);

         // 3. Valida desconto
            Assert.True(game.OnSale);
  Assert.True(game.FinalPrice < game.Price);
     Assert.NotEmpty(game.PromotionDescription);

            // 4. Calcula desconto esperado
          decimal descontoEsperado = game.Price - game.FinalPrice;
       Assert.True(descontoEsperado > 0);
 }
    }

    [Fact]
  public async Task FluxoCompleto_FiltrosEOrdenacaoCombinados_DeveFuncionarCorretamente()
    {
        // 1. Filtra por faixa de pre�o e ordena por pre�o
     var response = await ApiClient.GetAsync(
   "/api/gallery?PageNumber=1&PageSize=10&Filter.MinPrice=20&Filter.MaxPrice=100&OrderBy.OrderBy=Price&OrderBy.Ascending=true",
     cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(response);
        Assert.NotNull(result);

        // 2. Verifica se todos os jogos est�o na faixa de pre�o
        foreach (var game in result.Items)
      {
            Assert.True(game.FinalPrice >= 20);
     Assert.True(game.FinalPrice <= 100);
        }

 // 3. Verifica se est� ordenado
        if (result.Items.Count() > 1)
    {
            var games = result.Items.ToList();
  for (int i = 0; i < games.Count - 1; i++)
        {
           Assert.True(games[i].FinalPrice <= games[i + 1].FinalPrice);
    }
        }
    }

    [Fact]
    public async Task FluxoCompleto_PaginacaoSequencial_DeveFuncionarCorretamente()
  {
        // 1. Obt�m primeira p�gina
        var page1Response = await ApiClient.GetAsync(
        "/api/gallery?PageNumber=1&PageSize=5",
            cancellationToken: TestContext.Current.CancellationToken);
        var page1 = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(page1Response);
        
        Assert.NotNull(page1);
        Assert.Equal(1, page1.PageNumber);
        Assert.Equal(5, page1.PageSize);

        // 2. Se houver segunda p�gina, obt�m
        if (page1.TotalPages > 1)
        {
 var page2Response = await ApiClient.GetAsync(
     "/api/gallery?PageNumber=2&PageSize=5",
 cancellationToken: TestContext.Current.CancellationToken);
   var page2 = await ReadServiceResultAsync<PagedResult<GalleryGameResponseDto>>(page2Response);
    
       Assert.NotNull(page2);
         Assert.Equal(2, page2.PageNumber);

  // 3. Verifica que os jogos s�o diferentes
  var page1Ids = page1.Items.Select(g => g.Id).ToList();
            var page2Ids = page2.Items.Select(g => g.Id).ToList();
            
            Assert.Empty(page1Ids.Intersect(page2Ids));
        }
    }

    #endregion
}
