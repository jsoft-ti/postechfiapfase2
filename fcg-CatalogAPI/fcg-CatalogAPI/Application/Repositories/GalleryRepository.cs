using Application.Interfaces.Repository;
using Domain.Data;
using Domain.Data.Contexts;
using Domain.Entities;
using Domain.ValuesObjects;

namespace Application.Repositories;

public class GalleryRepository : IGalleryRepository
{
    private readonly IDAL<GalleryGame> _dal;
    private readonly FcgDbContext _context;

    public GalleryRepository(IDAL<GalleryGame> dal, FcgDbContext context)
    {
        _dal = dal;
        _context = context;
    }

    public async Task<bool> OwnsGalleryGameAsync(string EAN)
    {
        var game = await _dal.FindAsync(g => g.Game.EAN == EAN);
        return game != null;
    }

    public async Task<bool> OwnsGalleryGameAsync(Guid id)
    {
        var game = await _dal.FindAsync(g => g.Id == id);
        return game != null;
    }

    public async Task<GalleryGame> AddToGalleryGameAsync(GalleryGame game)
    {
        await _dal.AddAsync(game);
        await SaveChangesAsync();
        return game;
    }

    public async Task<GalleryGame?> GetGalleryGameByIdAsync(Guid id)
    {
        return await _dal.FindAsync(g => g.Id == id, g => g.Promotion, g => g.Game);
    }

    public async Task<GalleryGame?> GetByGameIdAsync(Guid gameId)
    {
        return await _dal.FindAsync(g => g.GameId == gameId, g => g.Promotion, g => g.Game);
    }

    public async Task<IEnumerable<GalleryGame>> GetAllGalleryGamesAsync()
    {
        return await _dal.ListAsync(g => g.Promotion, g => g.Game);
    }

    public async Task UpdateGalleryGameAsync(GalleryGame game)
    {
        await _dal.UpdateAsync(game);
        await SaveChangesAsync();
    }

    public async Task RemoveFromGalleryGameAsync(GalleryGame game)
    {
        await _dal.DeleteAsync(game);
        await SaveChangesAsync();
    }

    public async Task<IEnumerable<GalleryGame>> GetPromotionalGalleryGameAsync()
    {
        return await _dal.FindListAsync(
            g => g.Promotion != null && g.Promotion != Promotion.None,
            g => g.Promotion, g => g.Game);
    }

    public async Task<IEnumerable<GalleryGame>> GetGalleryGameByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        return await _dal.FindListAsync(
            g => g.Price >= minPrice && g.Price <= maxPrice,
         g => g.Promotion, g => g.Game);
    }

    public async Task<bool> IsAvailableForPurchaseAsync(Guid id)
    {
        var gallery = await GetGalleryGameByIdAsync(id);
        return gallery != null;
    }

    public async Task<IEnumerable<GalleryGame>> GetGalleryGamesByGenreAsync(string genre)
    {
        return await _dal.FindListAsync(
                g => g.Game.Genre.ToLower() == genre.ToLower(),
                g => g.Promotion, g => g.Game);
    }

    public async Task<IEnumerable<GalleryGame>> SearchGalleryGamesAsync(string searchTerm)
    {
        searchTerm = searchTerm.ToLower();
        return await _dal.FindListAsync(
          g => g.Game.Title.ToLower().Contains(searchTerm) ||
           g.Game.Genre.ToLower().Contains(searchTerm) ||
     (g.Game.Description != null && g.Game.Description.ToLower().Contains(searchTerm)),
g => g.Promotion, g => g.Game);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}