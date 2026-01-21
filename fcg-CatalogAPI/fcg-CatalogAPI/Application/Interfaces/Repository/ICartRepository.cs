using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface ICartRepository
{
    Task<Cart?> GetByPlayerIdAsync(Guid playerId);
    Task<Cart> GetOrCreateByPlayerIdAsync(Guid playerId);
    Task AddAsync(Cart cart);
    Task UpdateAsync(Cart cart);
    Task RemoveAsync(Guid cartId);
    Task<bool> OwnsCartAsync(Guid playerId);
    Task<bool> OwnsItemAsync(Guid playerId, Guid galleryId);
}

