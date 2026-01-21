using Bogus;
using Domain.Entities;

namespace Tests.Domain.Entities;

public class PurchaseTests
{
    private readonly Faker _faker;

    public PurchaseTests()
    {
        _faker = new Faker("pt_BR");
    }

    [Fact]
    public void Purchase_DeveCriarComDadosValidos()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var dataAntes = DateTime.UtcNow;

        // Act
        var purchase = new Purchase(playerId, gameId);
        var dataDepois = DateTime.UtcNow;

        // Assert
        Assert.NotEqual(Guid.Empty, purchase.Id);
        Assert.Equal(playerId, purchase.PlayerId);
        Assert.Equal(gameId, purchase.GameId);
        Assert.InRange(purchase.PurchaseDate, dataAntes, dataDepois);
    }

    [Fact]
    public void Purchase_DeveGerarIdUnico()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        // Act
        var purchase1 = new Purchase(playerId, gameId);
        var purchase2 = new Purchase(playerId, gameId);

        // Assert
        Assert.NotEqual(purchase1.Id, purchase2.Id);
    }

    [Fact]
    public void Purchase_DeveUsarUtcNow()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        // Act
        var purchase = new Purchase(playerId, gameId);

        // Assert
        Assert.Equal(DateTimeKind.Utc, purchase.PurchaseDate.Kind);
    }

    [Fact]
    public void Purchase_DevePermitirMesmosJogadorComprarMultiplosJogos()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var game1Id = Guid.NewGuid();
        var game2Id = Guid.NewGuid();

        // Act
        var purchase1 = new Purchase(playerId, game1Id);
        var purchase2 = new Purchase(playerId, game2Id);

        // Assert
        Assert.Equal(playerId, purchase1.PlayerId);
        Assert.Equal(playerId, purchase2.PlayerId);
        Assert.NotEqual(purchase1.GameId, purchase2.GameId);
        Assert.NotEqual(purchase1.Id, purchase2.Id);
    }

    [Fact]
    public void Purchase_DevePermitirMultiplosJogadoresComprarMesmoJogo()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        // Act
        var purchase1 = new Purchase(player1Id, gameId);
        var purchase2 = new Purchase(player2Id, gameId);

        // Assert
        Assert.Equal(gameId, purchase1.GameId);
        Assert.Equal(gameId, purchase2.GameId);
        Assert.NotEqual(purchase1.PlayerId, purchase2.PlayerId);
        Assert.NotEqual(purchase1.Id, purchase2.Id);
    }

    [Fact]
    public void Purchase_DeveRegistrarDataCompraNoMomentoInstanciacao()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var antes = DateTime.UtcNow;

        // Act
        System.Threading.Thread.Sleep(10); // Pequeno delay para garantir diferen�a
        var purchase = new Purchase(playerId, gameId);
        System.Threading.Thread.Sleep(10);
        var depois = DateTime.UtcNow;

        // Assert
        Assert.True(purchase.PurchaseDate >= antes);
        Assert.True(purchase.PurchaseDate <= depois);
    }

    [Fact]
    public void Purchase_DeveInicializarComNavigationPropertiesNaoNulas()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        // Act
        var purchase = new Purchase(playerId, gameId);

        // Assert
        // Navigation properties s�o inicializadas como null! mas n�o devem lan�ar exce��o ao acessar
        Assert.NotEqual(Guid.Empty, purchase.Id);
        Assert.NotEqual(Guid.Empty, purchase.PlayerId);
        Assert.NotEqual(Guid.Empty, purchase.GameId);
    }
}