using Domain.Entities;

namespace Tests.Domain.Entities;

/// <summary>
/// Testes unit�rios para a entidade Cart
/// </summary>
public class CartTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_DeveCriarCarrinhoValido_QuandoPlayerIdValido()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        // Act
        var cart = new Cart(playerId);

        // Assert
        Assert.NotEqual(Guid.Empty, cart.Id);
        Assert.Equal(playerId, cart.PlayerId);
        Assert.NotNull(cart.Items);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void Constructor_DeveLancarExcecao_QuandoPlayerIdVazio()
    {
        // Arrange
        var playerId = Guid.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Cart(playerId));
        Assert.Contains("PlayerId cannot be empty", exception.Message);
    }

    #endregion

    #region AddItem Tests

    [Fact]
    public void AddItem_DeveAdicionarJogo_QuandoJogoValido()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var cart = new Cart(playerId);
        var game = new Game("7891234560001", "Test Game", "Action", "Description");

        var gallery = new GalleryGame(game, 99.95m);

        // Act
        cart.AddItem(gallery);

        // Assert
        Assert.Single(cart.Items);
        Assert.Contains(cart.Items, item => item.GalleryId == game.Id);
    }

    [Fact]
    public void AddItem_DeveAdicionarMultiplosJogos_QuandoJogosDiferentes()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var cart = new Cart(playerId);
        var game1 = new Game("7891234560001", "Game 1", "Action", "Description 1");
        var game2 = new Game("7891234560002", "Game 2", "RPG", "Description 2");

        // Act        
        cart.AddItem(new GalleryGame(game1, 99.95m));
        cart.AddItem(new GalleryGame(game2, 199.95m));

        // Assert
        Assert.Equal(2, cart.Items.Count);
        Assert.Contains(cart.Items, item => item.GalleryId == game1.Id);
        Assert.Contains(cart.Items, item => item.GalleryId == game2.Id);
    }

    [Fact]
    public void AddItem_NaoDeveAdicionarJogoDuplicado_QuandoJogoJaExiste()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var cart = new Cart(playerId);
        var game = new Game("7891234560001", "Test Game", "Action", "Description");

        // Act
        cart.AddItem(new GalleryGame(game, 99.95m));
        cart.AddItem(new GalleryGame(game, 99.95m));

        // Assert
        Assert.Single(cart.Items); // Deve continuar com apenas 1 item
    }

    [Fact]
    public void AddItem_DeveLancarExcecao_QuandoJogoNulo()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var cart = new Cart(playerId);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => cart.AddItem(null!));
    }

    [Fact]
    public void AddItem_DeveLancarExcecao_QuandoGameIdVazio()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var cart = new Cart(playerId);

        // Cria um jogo com construtor padr�o (Id ser� Guid.Empty)
        var gameType = typeof(Game);
        var game = (Game)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(gameType);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => cart.AddItem(new GalleryGame(game, 99.95m)));
        Assert.Contains("Game Id cannot be empty", exception.Message);
    }

    #endregion

    #region RemoveItem Tests

    [Fact]
    public void RemoveItem_DeveRemoverJogo_QuandoJogoExiste()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var cart = new Cart(playerId);
        var game = new Game("7891234560001", "Test Game", "Action", "Description");
        cart.AddItem(new GalleryGame(game, 99.95m));

        // Act
        cart.RemoveItem(game.Id);

        // Assert
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void RemoveItem_DeveRemoverApenasJogoEspecifico_QuandoMultiplosJogos()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var cart = new Cart(playerId);
        var game1 = new Game("7891234560001", "Game 1", "Action", "Description 1");
        var game2 = new Game("7891234560002", "Game 2", "RPG", "Description 2");
        var game3 = new Game("7891234560003", "Game 3", "Strategy", "Description 3");

        cart.AddItem(new GalleryGame(game1, 99.99m));
        cart.AddItem(new GalleryGame(game2, 199.98m));
        cart.AddItem(new GalleryGame(game3, 299.97m));

        // Act
        cart.RemoveItem(game2.Id);

        // Assert
        Assert.Equal(2, cart.Items.Count);
        Assert.Contains(cart.Items, item => item.GalleryId == game1.Id);
        Assert.DoesNotContain(cart.Items, item => item.GalleryId == game2.Id);
        Assert.Contains(cart.Items, item => item.GalleryId == game3.Id);
    }

    [Fact]
    public void RemoveItem_NaoDeveFazerNada_QuandoJogoNaoExiste()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var cart = new Cart(playerId);
        var game = new Game("7891234560001", "Test Game", "Action", "Description");
        cart.AddItem(new GalleryGame(game, 99.95m));
        var gameIdInexistente = Guid.NewGuid();

        // Act
        cart.RemoveItem(gameIdInexistente);

        // Assert
        Assert.Single(cart.Items); // Deve manter o item original
    }

    [Fact]
    public void RemoveItem_NaoDeveLancarExcecao_QuandoCarrinhoVazio()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var cart = new Cart(playerId);
        var gameId = Guid.NewGuid();

        // Act & Assert
        var exception = Record.Exception(() => cart.RemoveItem(gameId));
        Assert.Null(exception);
        Assert.Empty(cart.Items);
    }

    #endregion

    #region Clear Tests

    [Fact]
    public void Clear_DeveLimparTodosOsItens_QuandoCarrinhoTemItens()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var cart = new Cart(playerId);
        var game1 = new Game("7891234560001", "Game 1", "Action", "Description 1");
        var game2 = new Game("7891234560002", "Game 2", "RPG", "Description 2");
        var game3 = new Game("7891234560003", "Game 3", "Strategy", "Description 3");

        cart.AddItem(new GalleryGame(game1, 99.99m));
        cart.AddItem(new GalleryGame(game2, 199.98m));
        cart.AddItem(new GalleryGame(game3, 299.97m));

        // Act
        cart.Clear();

        // Assert
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void Clear_NaoDeveLancarExcecao_QuandoCarrinhoVazio()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var cart = new Cart(playerId);

        // Act & Assert
        var exception = Record.Exception(() => cart.Clear());
        Assert.Null(exception);
        Assert.Empty(cart.Items);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FluxoCompleto_AdicionarRemoverLimpar_DeveFuncionarCorretamente()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var cart = new Cart(playerId);
        var game1 = new Game("7891234560001", "Game 1", "Action", "Description 1");
        var game2 = new Game("7891234560002", "Game 2", "RPG", "Description 2");
        var game3 = new Game("7891234560003", "Game 3", "Strategy", "Description 3");

        // Act & Assert - Adiciona itens
        cart.AddItem(new GalleryGame(game1, 99.99m));
        cart.AddItem(new GalleryGame(game2, 199.98m));
        cart.AddItem(new GalleryGame(game3, 299.97m));
        Assert.Equal(3, cart.Items.Count);

        // Remove um item
        cart.RemoveItem(game2.Id);
        Assert.Equal(2, cart.Items.Count);
        Assert.DoesNotContain(cart.Items, item => item.GalleryId == game2.Id);

        // Adiciona novamente
        cart.AddItem(new GalleryGame(game2, 99.95m));
        Assert.Equal(3, cart.Items.Count);

        // Limpa tudo
        cart.Clear();
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void Items_DeveRetornarColecaoReadOnly()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var cart = new Cart(playerId);
        var game = new Game("7891234560001", "Test Game", "Action", "Description");
        cart.AddItem(new GalleryGame(game, 99.95m));

        // Act
        var items = cart.Items;

        // Assert
        Assert.IsAssignableFrom<IReadOnlyCollection<CartItem>>(items);
        Assert.Single(items);
    }

    #endregion
}
