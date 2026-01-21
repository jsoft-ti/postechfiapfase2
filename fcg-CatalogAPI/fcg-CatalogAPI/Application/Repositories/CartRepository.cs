using Application.Interfaces.Repository;
using Domain.Data;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Repositories;

public class CartRepository : ICartRepository
{
    private readonly IDAL<Cart> _dal;

    public CartRepository(IDAL<Cart> dal)
    {
        _dal = dal;
    }

    public async Task<Cart?> GetByPlayerIdAsync(Guid playerId)
    {
        return await _dal.Query()
            .Where(c => c.PlayerId == playerId)
            .Include(c => c.Items)
                .ThenInclude(i => i.Gallery)
            .AsTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<Cart> GetOrCreateByPlayerIdAsync(Guid playerId)
    {
        var cart = await GetByPlayerIdAsync(playerId);

        if (cart is null)
        {
            cart = new Cart(playerId);
            await AddAsync(cart);
        }

        return cart;
    }

    public async Task AddAsync(Cart cart)
    {
        await _dal.AddAsync(cart);
    }

    public async Task UpdateAsync(Cart cart)
    {
        await _dal.UpdateAsync(cart);
    }

    public async Task RemoveAsync(Guid cartId)
    {
        var cart = await _dal.FindAsync(c => c.Id == cartId);
        if (cart != null)
            await _dal.DeleteAsync(cart);
    }

    public async Task<bool> OwnsItemAsync(Guid playerId, Guid galleryId)
    {
        var cart = await _dal.Query()
            .Where(c => c.PlayerId == playerId &&
                        c.Items.Any(i => i.GalleryId == galleryId))
            .Include(c => c.Items)
                .ThenInclude(i => i.Gallery)
            .FirstOrDefaultAsync();

        return cart != null;
    }

    public async Task<bool> OwnsCartAsync(Guid playerId)
    {
        return await _dal.Query()
            .AnyAsync(c => c.PlayerId == playerId);
    }
}
