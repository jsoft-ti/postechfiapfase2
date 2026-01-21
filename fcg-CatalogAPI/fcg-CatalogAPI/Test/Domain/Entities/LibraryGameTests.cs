using Domain.Entities;
using System.Numerics;
using YamlDotNet.Serialization.NodeTypeResolvers;

namespace Tests.Domain.Entities;

/// <summary>
/// Testes unit�rios para a entidade LibraryGame
/// </summary>
public class LibraryGameTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_DeveCriarLibraryGameValido_QuandoParametrosValidos()
    {
        // Arrange
        decimal purchasePrice = 49.99m;
        var game = new Game("7891234560001", "Test Game", "Action", "Description");
        var gallery = new GalleryGame(game, purchasePrice);
        var player = new Player(Guid.NewGuid(), "TestPlayer");

        // Act
        var libraryGame = new LibraryGame(gallery.Id, player.Id, purchasePrice);

        // Assert
        Assert.NotEqual(Guid.Empty, libraryGame.Id);
        Assert.Equal(gallery.Id, libraryGame.GalleryId);
        Assert.Equal(gallery, libraryGame.Gallery);
        Assert.Equal(player.Id, libraryGame.PlayerId);
        Assert.Equal(player, libraryGame.Player);
        Assert.Equal(purchasePrice, libraryGame.PurchasePrice);
        Assert.True(libraryGame.PurchaseDate <= DateTime.UtcNow);
        Assert.True(libraryGame.PurchaseDate >= DateTime.UtcNow.AddSeconds(-5)); // Margem de 5 segundos
    }

    [Fact]
    public void Constructor_DeveLancarExcecao_QuandoPrecoNegativo()
    {
        // Arrange
        decimal price = 59.99m;
        var game = new Game("7891234560001", "Test Game", "Action", "Description");
        var gallery = new GalleryGame(game, price);
        var player = new Player(Guid.NewGuid(), "TestPlayer");
        decimal precoInvalido = -10m;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new LibraryGame(gallery.Id, player.Id, precoInvalido));
        Assert.Contains("Pre�o de compra inv�lido", exception.Message);
    }

    [Fact]
    public void Constructor_DeveAceitarPrecoZero()
    {
        // Arrange
        decimal precoZero = 0m;
        var game = new Game("7891234560001", "Free Game", "Action", "Description");
        var gallery = new GalleryGame(game, precoZero);
        var player = new Player(Guid.NewGuid(), "TestPlayer");

        // Act
        var libraryGame = new LibraryGame(gallery.Id, player.Id, precoZero);

        // Assert
        Assert.Equal(0m, libraryGame.PurchasePrice);
    }

    [Fact]
    public void Constructor_DeveDefinirDataDeCompra_ComoDataAtual()
    {
        // Arrange
        decimal price = 59.99m;
        var game = new Game("7891234560001", "Test Game", "Action", "Description");
        var gallery = new GalleryGame(game, price);
        var player = new Player(Guid.NewGuid(), "TestPlayer");
        var beforeCreation = DateTime.UtcNow;

        // Act
        var libraryGame = new LibraryGame(gallery.Id, player.Id, price);
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.InRange(libraryGame.PurchaseDate, beforeCreation, afterCreation);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Properties_DeveArmazenarValoresCorretamente()
    {
        // Arrange
        decimal price = 99.99m;
        var gameId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var game = new Game("7891234560001", "Epic Game", "RPG", "Epic description");
        var gallery = new GalleryGame(game, price);
        var player = new Player(playerId, "EpicPlayer");

        // Act
        var libraryGame = new LibraryGame(gallery.Id, player.Id, price);

        // Assert
        Assert.Equal(game.Id, libraryGame.GalleryId);
        Assert.Equal(player.Id, libraryGame.PlayerId);
        Assert.Equal(price, libraryGame.PurchasePrice);
        Assert.NotNull(libraryGame.Gallery);
        Assert.NotNull(libraryGame.Player);
    }

    [Fact]
    public void PurchaseDate_DeveSerImmutable()
    {
        // Arrange
        decimal price = 59.99m;
        var game = new Game("7891234560001", "Test Game", "Action", "Description");
        var gallery = new GalleryGame(game, price);
        var player = new Player(Guid.NewGuid(), "TestPlayer");
        var libraryGame = new LibraryGame(gallery.Id, player.Id, price);
        var originalDate = libraryGame.PurchaseDate;

        // Act
        System.Threading.Thread.Sleep(100); // Pequeno delay

        // Assert
        Assert.Equal(originalDate, libraryGame.PurchaseDate); // N�o deve mudar
    }

    #endregion

    #region GetDescription Tests

    [Fact]
    public void GetDescription_DeveRetornarDescricaoCompleta()
    {
        // Arrange
        decimal price = 69.99m;
        var game = new Game("7891234560001", "Amazing Game", "Action", "Super fun game");
        var gallery = new GalleryGame(game, price);
        var player = new Player(Guid.NewGuid(), "GamerPro");
        var libraryGame = new LibraryGame(gallery.Id, player.Id, price);

        // Act
        var description = libraryGame.GetDescription();

        // Assert
        Assert.Contains("Amazing Game", description);
        Assert.Contains("79,90", description); // Pre�o formatado
        Assert.Contains("Comprado em", description);
    }

    [Fact]
    public void GetDescription_DeveIncluirDataDeCompra()
    {
        decimal price = 49.99m;
        // Arrange
        var game = new Game("7891234560001", "Test Game", "Action", "Description");
        var gallery = new GalleryGame(game, price);
        var player = new Player(Guid.NewGuid(), "TestPlayer");
        var libraryGame = new LibraryGame(gallery.Id, player.Id, 49.99m);

        // Act
        var description = libraryGame.GetDescription();

        // Assert
        Assert.Contains(libraryGame.PurchaseDate.ToString("d"), description);
    }

    [Fact]
    public void GetDescription_DeveIncluirPrecoDeCompra()
    {
        // Arrange
        var game = new Game("7891234560001", "Test Game", "Action", "Description");
        decimal price = 129.99m;
        var gallery = new GalleryGame(game, price);
        var player = new Player(Guid.NewGuid(), "TestPlayer");
        var libraryGame = new LibraryGame(gallery.Id, player.Id, price);

        // Act
        var description = libraryGame.GetDescription();

        // Assert
        Assert.Contains("129,99", description); // Formato brasileiro
    }

    [Fact]
    public void GetDescription_DeveFuncionarComJogoComSubtitulo()
    {
        // Arrange
        decimal price = 199.99m;
        var game = new Game("7891234560001", "Epic Saga", "RPG", "The best RPG ever", "The Beginning");
        var gallery = new GalleryGame(game, price);
        var player = new Player(Guid.NewGuid(), "RPGFan");
        var libraryGame = new LibraryGame(gallery.Id, player.Id, price);

        // Act
        var description = libraryGame.GetDescription();

        // Assert
        Assert.Contains("Epic Saga", description);
        Assert.Contains("The Beginning", description);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void MultiplasBibliotecas_DevemTerJogosIndependentes()
    {
        // Arrange
        decimal price1 = 49.99m;
        decimal price2 = 49.99m;
        decimal price3 = 49.99m;

        var game1 = new Game("7891234560001", "Game 1", "Action", "Description 1");
        var game2 = new Game("7891234560002", "Game 2", "RPG", "Description 2");

        var player1 = new Player(Guid.NewGuid(), "Player1");
        var player2 = new Player(Guid.NewGuid(), "Player2");

        var gallery2 = new GalleryGame(game1, price1);
        var gallery1 = new GalleryGame(game2, price2);

        // Act
        var library1Game1 = new LibraryGame(gallery1.Id, player1.Id, price1);
        var library1Game2 = new LibraryGame(gallery2.Id, player1.Id, price2);
        var library2Game1 = new LibraryGame(gallery1.Id, player2.Id, price3); // Mesmo jogo, pre�o diferente

        // Assert
        Assert.Equal(player1.Id, library1Game1.PlayerId);
        Assert.Equal(player1.Id, library1Game2.PlayerId);
        Assert.Equal(player2.Id, library2Game1.PlayerId);

        Assert.Equal(49.99m, library1Game1.PurchasePrice);
        Assert.Equal(39.99m, library2Game1.PurchasePrice); // Pre�os diferentes

        Assert.Equal(game1.Id, library1Game1.GalleryId);
        Assert.Equal(game1.Id, library2Game1.GalleryId); // Mesmo jogo
    }

    [Fact]
    public void JogoCompradoEmPromocao_DeveRegistrarPrecoPromocional()
    {
        // Arrange
        decimal promotionalPrice = 49.99m; // 50% off
        decimal originalPrice = 100m;
        var game = new Game("7891234560001", "Sale Game", "Action", "Description");
        var gallery = new GalleryGame(game, promotionalPrice);
        var player = new Player(Guid.NewGuid(), "BargainHunter");

        // Act
        var libraryGame = new LibraryGame(gallery.Id, player.Id, promotionalPrice);

        // Assert
        Assert.Equal(promotionalPrice, libraryGame.PurchasePrice);
        Assert.NotEqual(originalPrice, libraryGame.PurchasePrice);
    }

    [Fact]
    public void ComparacaoDePrecos_EntreDiferentesCompras()
    {
        // Arrange
        decimal priceFull = 199.90m;
        decimal pricePromo = 99.99m;
        var game = new Game("7891234560001", "Popular Game", "Action", "Description");
        var gallery = new GalleryGame(game, priceFull);
        var player1 = new Player(Guid.NewGuid(), "EarlyBuyer");
        var player2 = new Player(Guid.NewGuid(), "LateBuyer");

        // Act - Player 1 compra no lan�amento
        var earlyPurchase = new LibraryGame(gallery.Id, player1.Id, priceFull);

        System.Threading.Thread.Sleep(100); // Simula tempo passando

        // Player 2 compra em promo��o
        var latePurchase = new LibraryGame(gallery.Id, player2.Id, pricePromo);

        // Assert
        Assert.True(earlyPurchase.PurchasePrice > latePurchase.PurchasePrice);
        Assert.True(earlyPurchase.PurchaseDate < latePurchase.PurchaseDate);
        Assert.Equal(game.Id, earlyPurchase.GalleryId);
        Assert.Equal(game.Id, latePurchase.GalleryId);
    }

    [Fact]
    public void BibliotecaCompleta_DeveManterHistoricoDeCompras()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "Collector");
        var games = new[]
           {
                new Game("7891234560001", "Game 1", "Action", "Desc 1"),
                new Game("7891234560002", "Game 2", "RPG", "Desc 2"),
                new Game("7891234560003", "Game 3", "Strategy", "Desc 3")
            };

        var prices = new[] { 49.99m, 79.99m, 99.99m };

        // Act
        var libraryGames = new List<LibraryGame>();
        for (int i = 0; i < games.Length; i++)
        {
            var gallery = new GalleryGame(games[i], prices[i]);
            System.Threading.Thread.Sleep(10); // Pequeno delay entre compras
            libraryGames.Add(new LibraryGame(gallery.Id, player.Id, prices[i]));
        }

        // Assert
        Assert.Equal(3, libraryGames.Count);

        // Verifica ordem cronol�gica
        Assert.True(libraryGames[0].PurchaseDate <= libraryGames[1].PurchaseDate);
        Assert.True(libraryGames[1].PurchaseDate <= libraryGames[2].PurchaseDate);

        // Verifica total gasto
        var totalSpent = libraryGames.Sum(lg => lg.PurchasePrice);
        Assert.Equal(229.97m, totalSpent);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void LibraryGame_DeveAceitarJogoGratis()
    {
        // Arrange
        var game = new Game("7891234560001", "Free to Play", "Action", "Free game");
        var gallery = new GalleryGame(game, 109.0m);
        var player = new Player(Guid.NewGuid(), "FreebieCollector");

        // Act
        var libraryGame = new LibraryGame(gallery.Id, player.Id, 0m);

        // Assert
        Assert.Equal(0m, libraryGame.PurchasePrice);
        var description = libraryGame.GetDescription();
        Assert.Contains("0,00", description);
    }

    [Fact]
    public void LibraryGame_DeveAceitarPrecosAltos()
    {
        // Arrange
        decimal premiumPrice = 9999.99m;
        var game = new Game("7891234560001", "Premium Edition", "RPG", "Deluxe version");
        var gallery = new GalleryGame(game, premiumPrice);
        var player = new Player(Guid.NewGuid(), "WhaleGamer");

        // Act
        var libraryGame = new LibraryGame(gallery.Id, player.Id, premiumPrice);

        // Assert
        Assert.Equal(premiumPrice, libraryGame.PurchasePrice);
    }

    [Fact]
    public void LibraryGame_DeveManterReferenciaParaGameEPlayer()
    {
        // Arrange
        decimal price = 49.99m;
        var game = new Game("7891234560001", "Test Game", "Action", "Description");
        var gallery = new GalleryGame(game, price);
        var player = new Player(Guid.NewGuid(), "TestPlayer");

        // Act
        var libraryGame = new LibraryGame(gallery.Id, player.Id, price);

        // Assert
        Assert.Same(game, libraryGame.Gallery); // Mesma refer�ncia
        Assert.Same(player, libraryGame.Player); // Mesma refer�ncia
        Assert.Equal(game.Title, libraryGame.Gallery.Game.Title);
        Assert.Equal(player.DisplayName, libraryGame.Player.DisplayName);
    }

    #endregion
}
