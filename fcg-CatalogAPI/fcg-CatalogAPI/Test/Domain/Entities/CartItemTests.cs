using Bogus;
using Domain.Entities;

namespace Tests.Domain.Entities;

public class CartItemTests
{
    private readonly Faker _faker;

    public CartItemTests()
    {
        _faker = new Faker("pt_BR");
    }

    [Fact]
    public void CartItem_DeveCriarComDadosValidos()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var cartId = Guid.NewGuid();

        // Act
        var cartItem = new CartItem(playerId, gameId, cartId);

        // Assert
        Assert.NotEqual(Guid.Empty, cartItem.Id);
        Assert.Equal(playerId, cartItem.PlayerId);
        Assert.Equal(gameId, cartItem.GalleryId);
        Assert.Equal(cartId, cartItem.CartId);
    }

    [Fact]
    public void CartItem_DeveGerarIdUnico()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var cartId = Guid.NewGuid();

        // Act
        var cartItem1 = new CartItem(playerId, gameId, cartId);
        var cartItem2 = new CartItem(playerId, gameId, cartId);

        // Assert
        Assert.NotEqual(cartItem1.Id, cartItem2.Id);
    }

    [Fact]
    public void CartItem_DevePermitirMultiplosItensParaMesmoCarrinho()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var cartId = Guid.NewGuid();
        var game1Id = Guid.NewGuid();
        var game2Id = Guid.NewGuid();
        var game3Id = Guid.NewGuid();

        // Act
        var cartItem1 = new CartItem(playerId, game1Id, cartId);
        var cartItem2 = new CartItem(playerId, game2Id, cartId);
        var cartItem3 = new CartItem(playerId, game3Id, cartId);

        // Assert
        Assert.Equal(cartId, cartItem1.CartId);
        Assert.Equal(cartId, cartItem2.CartId);
        Assert.Equal(cartId, cartItem3.CartId);
        Assert.NotEqual(cartItem1.GalleryId, cartItem2.GalleryId);
        Assert.NotEqual(cartItem2.GalleryId, cartItem3.GalleryId);
    }

    [Fact]
    public void CartItem_DevePermitirMesmoJogoEmCarrinhosDiferentes()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var cart1Id = Guid.NewGuid();
        var cart2Id = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        // Act
        var cartItem1 = new CartItem(playerId, gameId, cart1Id);
        var cartItem2 = new CartItem(playerId, gameId, cart2Id);

        // Assert
        Assert.Equal(gameId, cartItem1.GalleryId);
        Assert.Equal(gameId, cartItem2.GalleryId);
        Assert.NotEqual(cartItem1.CartId, cartItem2.CartId);
        Assert.NotEqual(cartItem1.Id, cartItem2.Id);
    }

    [Fact]
    public void CartItem_DeveManterConsistenciaEntreIdEReferencias()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var cartId = Guid.NewGuid();

        // Act
        var cartItem = new CartItem(playerId, gameId, cartId);

        // Assert
        Assert.Equal(playerId, cartItem.PlayerId);
        Assert.Equal(gameId, cartItem.GalleryId);
        Assert.Equal(cartId, cartItem.CartId);
    }

    [Fact]
    public void CartItem_DevePermitirCriacaoEmLote()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var cartId = Guid.NewGuid();
        var cartItemFaker = new Faker<CartItem>()
            .CustomInstantiator(f => new CartItem(
                playerId,
                Guid.NewGuid(),
                cartId
            ));

        // Act
        var cartItems = cartItemFaker.Generate(10);

        // Assert
        Assert.Equal(10, cartItems.Count);
        Assert.All(cartItems, item => 
        {
            Assert.Equal(playerId, item.PlayerId);
            Assert.Equal(cartId, item.CartId);
        });
        Assert.Equal(cartItems.Count, cartItems.Select(c => c.Id).Distinct().Count());
        Assert.Equal(cartItems.Count, cartItems.Select(c => c.GalleryId).Distinct().Count());
    }
}