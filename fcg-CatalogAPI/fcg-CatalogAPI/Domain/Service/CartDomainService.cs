using Domain.Entities;

namespace Domain.Service;

public class CartDomainService : ICartDomainService
{
    public Cart CreateCart(Guid playerId)
    {
        if (playerId == Guid.Empty)
            throw new ArgumentException("PlayerId cannot be empty", nameof(playerId));

        return new Cart(playerId);
    }

    public void AddItemToCart(Cart cart, GalleryGame gallery)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));
        if (gallery == null)
            throw new ArgumentNullException(nameof(gallery));

        cart.AddItem(gallery);
    }

    public void RemoveItemFromCart(Cart cart, Guid galleryId)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));
        if (galleryId == Guid.Empty)
            throw new ArgumentException("GameId cannot be empty", nameof(galleryId));

        cart.RemoveItem(galleryId);
    }

    public void ClearCart(Cart cart)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        cart.Clear();
    }
}