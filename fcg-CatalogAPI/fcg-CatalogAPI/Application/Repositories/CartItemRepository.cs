using Domain.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Repositories;

public class CartItemRepository : ICartItemRepository
{
    private readonly IDAL<CartItem> _dal;

    public CartItemRepository(IDAL<CartItem> dal)
    {
        _dal = dal;
    }

    public async Task<CartItem?> GetByIdAsync(Guid id)
    {
        return await _dal.Query()
            .Include(ci => ci.Cart)
            .Include(ci => ci.Player)
            .Include(ci => ci.Gallery)
            .FirstOrDefaultAsync(ci => ci.Id == id);
    }

    public async Task<IEnumerable<CartItem>> GetByCartIdAsync(Guid cartId)
    {
        return await _dal.Query()
            .Where(ci => ci.CartId == cartId)
            .Include(ci => ci.Gallery)
            .ToListAsync();
    }

    public async Task<CartItem?> GetByPlayerAndGalleryAsync(Guid playerId, Guid galleryId)
    {
        return await _dal.Query()
            .Where(ci => ci.PlayerId == playerId && ci.GalleryId == galleryId)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Guid playerId, Guid galleryId, Guid cartId)
    {
        var context = _dal.GetDbContext();
        await context.Set<CartItem>().AddAsync(new CartItem(playerId, galleryId, cartId));
        await context.SaveChangesAsync();
    }

    public async Task RemoveAsync(Guid id)
    {
        var context = _dal.GetDbContext();
        var entity = await context.Set<CartItem>().FindAsync(id);

        if (entity != null)
        {
            context.Set<CartItem>().Remove(entity);
            await context.SaveChangesAsync();
        }
    }

    public async Task RemoveAsync(Guid playerId, Guid galleryId, Guid cartId)
    {
        var context = _dal.GetDbContext();


        var cart = await _dal.FindAsync(c =>
                                        c.PlayerId == playerId
                                        && c.GalleryId == galleryId
                                        && c.Id == cartId);

        if (cart != null)
            await _dal.DeleteAsync(cart);
    }

    public async Task<bool> ExistsAsync(Guid playerId, Guid galleryId)
    {
        return await _dal.Query()
            .AnyAsync(ci => ci.PlayerId == playerId && ci.GalleryId == galleryId);
    }

    public async Task<bool> OwnsItemAsync(Guid playerId, Guid galleryId, Guid cartId)
    {
        return await _dal.Query()
            .AnyAsync(ci => ci.PlayerId == playerId &&
                            ci.GalleryId == galleryId &&
                            ci.CartId == cartId);
    }
}
