using Domain.Entities;
using Domain.ValuesObjects;
using Domain.Enums;

namespace Tests.Domain.Entities;

/// <summary>
/// Testes unit�rios para a entidade GalleryGame
/// </summary>
public class GalleryGameTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_DeveCriarGalleryGameValido_QuandoParametrosValidos()
    {
 // Arrange
        var game = new Game("7891234560001", "Test Game", "Action", "Description");
        decimal price = 99.99m;

        // Act
        var galleryGame = new GalleryGame(game, price);

// Assert
        Assert.NotEqual(Guid.Empty, galleryGame.Id);
        Assert.Equal(game.Id, galleryGame.GameId);
      Assert.Equal(game, galleryGame.Game);
        Assert.Equal(price, galleryGame.Price);
  Assert.Equal(price, galleryGame.FinalPrice); // Sem promo��o, pre�o final = pre�o original
        Assert.NotNull(galleryGame.Promotion);
    }

    [Fact]
    public void Constructor_DeveLancarExcecao_QuandoPrecoNegativo()
    {
      // Arrange
      var game = new Game("7891234560001", "Test Game", "Action", "Description");
decimal precoInvalido = -10m;

  // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new GalleryGame(game, precoInvalido));
        Assert.Contains("Pre�o inv�lido", exception.Message);
    }

    [Fact]
    public void Constructor_DeveAceitarPrecoZero()
    {
        // Arrange
     var game = new Game("7891234560001", "Free Game", "Action", "Description");
        decimal precoZero = 0m;

        // Act
        var galleryGame = new GalleryGame(game, precoZero);

        // Assert
        Assert.Equal(0m, galleryGame.Price);
        Assert.Equal(0m, galleryGame.FinalPrice);
    }

 #endregion

    #region FinalPrice Tests

    [Fact]
    public void FinalPrice_DeveRetornarPrecoOriginal_QuandoSemPromocao()
    {
// Arrange
        var game = new Game("7891234560001", "Test Game", "Action", "Description");
        decimal price = 100m;
        var galleryGame = new GalleryGame(game, price);

        // Act
        var finalPrice = galleryGame.FinalPrice;

   // Assert
    Assert.Equal(price, finalPrice);
    }

  [Fact]
  public void FinalPrice_DeveAplicarDescontoFixo_QuandoPromocaoAtiva()
    {
    // Arrange
 var game = new Game("7891234560001", "Test Game", "Action", "Description");
        decimal price = 100m;
        var galleryGame = new GalleryGame(game, price);

        var promotion = Promotion.Create(
 PromotionType.FixedDiscount,
 20m, // R$ 20 de desconto
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1)
        );

        // Act
        galleryGame.ApplyPromotion(promotion);

        // Assert
        Assert.Equal(80m, galleryGame.FinalPrice); // 100 - 20 = 80
    }

    [Fact]
    public void FinalPrice_DeveAplicarDescontoPercentual_QuandoPromocaoAtiva()
    {
        // Arrange
        var game = new Game("7891234560001", "Test Game", "Action", "Description");
        decimal price = 100m;
        var galleryGame = new GalleryGame(game, price);

      var promotion = Promotion.Create(
   PromotionType.PercentageDiscount,
      25m, // 25% de desconto
      DateTime.UtcNow.AddDays(-1),
 DateTime.UtcNow.AddDays(1)
        );

 // Act
    galleryGame.ApplyPromotion(promotion);

  // Assert
        Assert.Equal(75m, galleryGame.FinalPrice); // 100 * (1 - 0.25) = 75
    }

    [Fact]
    public void FinalPrice_DeveRetornarPrecoOriginal_QuandoPromocaoExpirada()
    {
        // Arrange
        var game = new Game("7891234560001", "Test Game", "Action", "Description");
        decimal price = 100m;
        var galleryGame = new GalleryGame(game, price);

        var promotion = Promotion.Create(
            PromotionType.FixedDiscount,
            20m,
            DateTime.UtcNow.AddDays(-10),
 DateTime.UtcNow.AddDays(-1) // Expirou ontem
        );

      // Act
        galleryGame.ApplyPromotion(promotion);

   // Assert
        Assert.Equal(price, galleryGame.FinalPrice);
    }

    [Fact]
    public void FinalPrice_NaoDeveRetornarNegativo_QuandoDescontoMaiorQuePreco()
    {
        // Arrange
   var game = new Game("7891234560001", "Test Game", "Action", "Description");
     decimal price = 50m;
        var galleryGame = new GalleryGame(game, price);

  var promotion = Promotion.Create(
            PromotionType.FixedDiscount,
            100m, // Desconto maior que o pre�o
       DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1)
        );

   // Act
        galleryGame.ApplyPromotion(promotion);

        // Assert
        Assert.True(galleryGame.FinalPrice >= 0); // N�o deve ser negativo
    }

    #endregion

    #region ApplyPromotion Tests

    [Fact]
    public void ApplyPromotion_DeveAplicarPromocao_QuandoPromocaoValida()
    {
        // Arrange
        var game = new Game("7891234560001", "Test Game", "Action", "Description");
        var galleryGame = new GalleryGame(game, 100m);

        var promotion = Promotion.Create(
    PromotionType.FixedDiscount,
    15m,
      DateTime.UtcNow,
            DateTime.UtcNow.AddDays(7)
        );

        // Act
  galleryGame.ApplyPromotion(promotion);

        // Assert
   Assert.Equal(promotion, galleryGame.Promotion);
        Assert.Equal(85m, galleryGame.FinalPrice);
    }

    [Fact]
    public void ApplyPromotion_DeveSubstituirPromocaoAnterior_QuandoNovaPromocao()
    {
        // Arrange
        var game = new Game("7891234560001", "Test Game", "Action", "Description");
        var galleryGame = new GalleryGame(game, 100m);

 var promotion1 = Promotion.Create(
            PromotionType.FixedDiscount,
            10m,
            DateTime.UtcNow,
  DateTime.UtcNow.AddDays(7)
 );

      var promotion2 = Promotion.Create(
      PromotionType.PercentageDiscount,
          20m,
        DateTime.UtcNow,
   DateTime.UtcNow.AddDays(7)
        );

     // Act
        galleryGame.ApplyPromotion(promotion1);
        var priceWithPromo1 = galleryGame.FinalPrice;

        galleryGame.ApplyPromotion(promotion2);
        var priceWithPromo2 = galleryGame.FinalPrice;

        // Assert
        Assert.Equal(90m, priceWithPromo1); // 100 - 10
        Assert.Equal(80m, priceWithPromo2); // 100 * 0.8
        Assert.Equal(promotion2, galleryGame.Promotion);
    }

    [Fact]
    public void ApplyPromotion_DeveAceitarPromocaoNula()
    {
        // Arrange
    var game = new Game("7891234560001", "Test Game", "Action", "Description");
        var galleryGame = new GalleryGame(game, 100m);

        // Act
     galleryGame.ApplyPromotion(null!);

   // Assert
        Assert.Equal(Promotion.None, galleryGame.Promotion);
        Assert.Equal(100m, galleryGame.FinalPrice);
    }

    [Fact]
    public void ApplyPromotion_DeveRemoverPromocao_QuandoPromotionNone()
    {
        // Arrange
        var game = new Game("7891234560001", "Test Game", "Action", "Description");
      var galleryGame = new GalleryGame(game, 100m);

        var promotion = Promotion.Create(
      PromotionType.FixedDiscount,
            20m,
            DateTime.UtcNow,
     DateTime.UtcNow.AddDays(7)
        );

        galleryGame.ApplyPromotion(promotion);
        Assert.Equal(80m, galleryGame.FinalPrice);

  // Act
   galleryGame.ApplyPromotion(Promotion.None);

        // Assert
        Assert.Equal(100m, galleryGame.FinalPrice);
    }

    #endregion

    #region GetDescription Tests

    [Fact]
    public void GetDescription_DeveRetornarDescricaoCompleta()
 {
    // Arrange
        var game = new Game("7891234560001", "Test Game", "Action", "Amazing game description");
  var galleryGame = new GalleryGame(game, 99.99m);

        // Act
    var description = galleryGame.GetDescription();

        // Assert
        Assert.Contains("Test Game", description);
        Assert.Contains("99,99", description); // Formato brasileiro
    }

    [Fact]
    public void GetDescription_DeveIncluirPrecoComPromocao_QuandoPromocaoAtiva()
    {
     // Arrange
        var game = new Game("7891234560001", "Test Game", "Action", "Description");
        var galleryGame = new GalleryGame(game, 100m);

        var promotion = Promotion.Create(
          PromotionType.FixedDiscount,
      30m,
DateTime.UtcNow,
      DateTime.UtcNow.AddDays(7)
        );

        galleryGame.ApplyPromotion(promotion);

        // Act
        var description = galleryGame.GetDescription();

        // Assert
    Assert.Contains("70,00", description); // Pre�o com desconto
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FluxoCompleto_CriarAplicarPromocaoRemover_DeveFuncionarCorretamente()
    {
        // Arrange
  var game = new Game("7891234560001", "Awesome Game", "RPG", "Epic adventure");
     var galleryGame = new GalleryGame(game, 150m);

        // Act & Assert - Sem promo��o
     Assert.Equal(150m, galleryGame.FinalPrice);

        // Aplica promo��o de 20%
        var promotion1 = Promotion.Create(
    PromotionType.PercentageDiscount,
        20m,
      DateTime.UtcNow,
       DateTime.UtcNow.AddDays(7)
        );
        galleryGame.ApplyPromotion(promotion1);
        Assert.Equal(120m, galleryGame.FinalPrice);

   // Substitui por promo��o de R$ 50
        var promotion2 = Promotion.Create(
            PromotionType.FixedDiscount,
     50m,
   DateTime.UtcNow,
            DateTime.UtcNow.AddDays(7)
        );
      galleryGame.ApplyPromotion(promotion2);
        Assert.Equal(100m, galleryGame.FinalPrice);

        // Remove promo��o
   galleryGame.ApplyPromotion(Promotion.None);
        Assert.Equal(150m, galleryGame.FinalPrice);
    }

    [Fact]
    public void MultiplasTrocasDePromocao_DeveManterConsistencia()
    {
   // Arrange
        var game = new Game("7891234560001", "Test Game", "Action", "Description");
        var galleryGame = new GalleryGame(game, 200m);

        var promotions = new[]
      {
            Promotion.Create(PromotionType.FixedDiscount, 20m, DateTime.UtcNow, DateTime.UtcNow.AddDays(7)),
  Promotion.Create(PromotionType.PercentageDiscount, 10m, DateTime.UtcNow, DateTime.UtcNow.AddDays(7)),
   Promotion.Create(PromotionType.FixedDiscount, 50m, DateTime.UtcNow, DateTime.UtcNow.AddDays(7)),
            Promotion.None
  };

        var expectedPrices = new[] { 180m, 180m, 150m, 200m };

        // Act & Assert
        for (int i = 0; i < promotions.Length; i++)
     {
            galleryGame.ApplyPromotion(promotions[i]);
            Assert.Equal(expectedPrices[i], galleryGame.FinalPrice);
    }
    }

    #endregion
}
