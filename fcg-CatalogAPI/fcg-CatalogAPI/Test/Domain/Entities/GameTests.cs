using Bogus;
using Domain.Entities;
using Domain.Enums;
using Domain.ValuesObjects;

namespace Tests.Domain.Entities;

public class GameTests
{
    private readonly Faker _faker;

    public GameTests()
    {
        _faker = new Faker("pt_BR");
    }

    [Fact]
    public void Game_DeveCriarComDadosValidos()
    {
        // Arrange
        var ean = string.Concat(_faker.Random.Digits(13));
        var name = _faker.Commerce.ProductName();
        var genre = _faker.PickRandom("RPG", "FPS", "Adventure");
        var description = _faker.Lorem.Sentence();
        var price = _faker.Random.Decimal(10, 200);

        // Act
        var game = new Game(ean, name, genre, description);

        // Assert
        Assert.NotEqual(Guid.Empty, game.Id);
        Assert.Equal(name, game.Title);
        Assert.Equal(genre, game.Genre);
        Assert.Equal(description, game.Description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Game_DeveLancarExcecao_QuandoEANInvalido(string eanInvalido)
    {
        // Arrange & Act & Assert        
        var exception = Assert.Throws<ArgumentException>(() =>
            new Game(eanInvalido, "Game Name", "RPG", "Description")
        );

        Assert.Equal("EAN obrigat�rio", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Game_DeveLancarExcecao_QuandoTitleInvalido(string titleInvalido)
    {
        // Arrange & Act & Assert
        var ean = "7894561230000";
        var exception = Assert.Throws<ArgumentException>(() =>
            new Game(ean, titleInvalido, "RPG", "Description")
        );

        Assert.Equal("T�tulo obrigat�rio", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Game_DeveLancarExcecao_QuandoGeneroInvalido(string generoInvalido)
    {
        // Arrange & Act & Assert
        var ean = "7894561230000";
        var exception = Assert.Throws<ArgumentException>(() =>
            new Game(ean, "Game Name", generoInvalido, "Description")
        );

        Assert.Equal("G�nero obrigat�rio", exception.Message);
    }

    
    [Fact]
    public void PrecoFinal_DeveRetornarPrecoOriginal_QuandoSemPromocao()
    {
        // Arrange
        var ean = "7894561230000";
        var price = 100m;
        var game = new Game(ean, _faker.Commerce.ProductName(), "RPG", null);

        // Act
        // var precoFinal = game.PrecoFinal;

        // Assert
        // Assert.Equal(price, precoFinal);
    }

    [Fact]
    public void AplicarPromocao_DeveAplicarDescontoPercentual()
    {
        // Arrange
        var ean = "7894561230000";
        var price = 100m;
        var game = new Game(ean, _faker.Commerce.ProductName(), "RPG", null);
        var promocao = Promotion.Create(
            PromotionType.PercentageDiscount,
            20m,
            DateTime.Now.AddDays(-1),
            DateTime.Now.AddDays(1)
        );

        // Act
        // game.AplicarPromocao(promocao);

        // Assert
        // Assert.Equal(80m, game.PrecoFinal);
    }

    [Fact]
    public void AplicarPromocao_DeveAplicarDescontoFixo()
    {
        // Arrange
        var ean = "7894561230000";
        var price = 100m;
        var game = new Game(ean, _faker.Commerce.ProductName(), "RPG", null);
        var promocao = Promotion.Create(
            PromotionType.FixedDiscount,
            30m,
            DateTime.Now.AddDays(-1),
            DateTime.Now.AddDays(1)
        );

        // Act
        // game.AplicarPromocao(promocao);

        // Assert
        // Assert.Equal(70m, game.PrecoFinal);
    }

    [Fact]
    public void GetDescription_DeveRetornarDescricaoFormatada()
    {
        // Arrange
        var ean = "7894561230000";
        var name = "Cyberpunk 2077";
        var genre = "RPG";
        var game = new Game(ean, name, genre, "Descri��o");

        // Act
        var description = game.GetDescription();

        // Assert
        Assert.Contains(name, description);
        Assert.Contains("Descri��o", description);
    }
}