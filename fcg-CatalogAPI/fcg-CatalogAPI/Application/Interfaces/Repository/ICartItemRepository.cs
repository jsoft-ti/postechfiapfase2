namespace Application.Repositories;

public interface ICartItemRepository
{
    Task<CartItem?> GetByIdAsync(Guid id);
    Task<IEnumerable<CartItem>> GetByCartIdAsync(Guid cartId);
    Task<CartItem?> GetByPlayerAndGalleryAsync(Guid playerId, Guid galleryId);
    Task AddAsync(Guid playerId, Guid galleryId, Guid cartId);
    Task RemoveAsync(Guid id);
    Task RemoveAsync(Guid playerId, Guid galleryId, Guid cartId);
    Task<bool> ExistsAsync(Guid playerId, Guid galleryId);
    Task<bool> OwnsItemAsync(Guid playerId, Guid galleryId, Guid cartId);
}
