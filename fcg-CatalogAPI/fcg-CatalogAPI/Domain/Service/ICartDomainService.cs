using Domain.Entities;

namespace Domain.Service;

public interface ICartDomainService
{
    Cart CreateCart(Guid playerId);
    void AddItemToCart(Cart cart, GalleryGame game);
    void RemoveItemFromCart(Cart cart, Guid galleryId);
    void ClearCart(Cart cart);
}