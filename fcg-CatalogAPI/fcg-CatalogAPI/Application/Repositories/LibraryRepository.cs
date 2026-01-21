using Application.Interfaces.Repository;
using Domain.Data;
using Domain.Data.Contexts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Repositories;

public class LibraryRepository : ILibraryRepository
{
    private readonly IDAL<LibraryGame> _dal;
    private readonly FcgDbContext _context;

    public LibraryRepository(IDAL<LibraryGame> dal, FcgDbContext context)
    {
        _dal = dal;
        _context = context;
    }

    public async Task<(bool, LibraryGame)> AddToLibraryAsync(LibraryGame game)
    {
        try
        {
            await _dal.AddAsync(game);
            await SaveChangesAsync();
            return (true, game);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return (false, game);
        }

    }

    public async Task<LibraryGame?> GetLibraryGameAsync(Guid playerId, Guid gameId)
    {
        return await _dal.FindAsync(
            g => g.PlayerId == playerId && g.Id == gameId,
            g => g.Player);
    }

    public async Task<IEnumerable<LibraryGame>> GetPlayerLibraryAsync(Guid playerId)
    {
        return await _dal.FindListAsync(
            g => g.PlayerId == playerId,
            g => g.Player);
    }

    public async Task<bool> OwnsLibraryGameAsync(Guid playerId, Guid gameId)
    {
        var game = await GetLibraryGameAsync(playerId, gameId);
        return game != null;
    }

    public async Task<bool> HasGameInLibraryAsync(Guid playerId, string gameName)
    {
        var game = await _dal.FindAsync(
            g => g.PlayerId == playerId && g.Gallery.Game.Title.ToLower() == gameName.ToLower());
        return game != null;
    }

    public async Task<IEnumerable<LibraryGame>> GetRecentPurchasesAsync(Guid playerId, int count)
    {
        var games = await _dal.FindListAsync(
            g => g.PlayerId == playerId,
            g => g.Player);

        return games.OrderByDescending(g => g.PurchaseDate).Take(count);
    }

    public async Task<IEnumerable<LibraryGame>> GetGamesByGenreAsync(Guid playerId, string genre)
    {
        return await _dal.FindListAsync(
            g => g.PlayerId == playerId && g.Gallery.Game.Genre.ToLower() == genre.ToLower(),
            g => g.Player);
    }

    public async Task<decimal> GetTotalSpentAsync(Guid playerId)
    {
        var games = await GetPlayerLibraryAsync(playerId);
        return games.Sum(g => g.PurchasePrice);
    }

    public async Task<int> GetLibraryGameCountAsync(Guid playerId)
    {
        var games = await GetPlayerLibraryAsync(playerId);
        return games.Count();
    }

    public async Task<IEnumerable<LibraryGame>> GetPurchasesByDateRangeAsync(Guid playerId, DateTime startDate, DateTime endDate)
    {
        return await _dal.FindListAsync(
            g => g.PlayerId == playerId &&
                 g.PurchaseDate >= startDate &&
                 g.PurchaseDate <= endDate,
            g => g.Player);
    }

    public async Task<decimal> GetSpentInPeriodAsync(Guid playerId, DateTime startDate, DateTime endDate)
    {
        var games = await GetPurchasesByDateRangeAsync(playerId, startDate, endDate);
        return games.Sum(g => g.PurchasePrice);
    }

    public async Task<IEnumerable<LibraryGame>> GetAllPurchasesAsync()
    {
        DateTime dataMin = DateTime.MinValue;
        var purchases = await _dal.FindListAsync(p => p.PurchaseDate >= dataMin);
        return purchases;
    }

    public IQueryable<LibraryGame> GetAllPurchasesQueryable()
    {
        return _dal.Query();
    }

    public async Task<int> GetTotalPurchaseCountAsync()
    {
        DateTime dataMin = DateTime.MinValue;
        var purchases = await _dal.FindListAsync(p => p.PurchaseDate >= dataMin);

        return purchases.Count;
    }

    public async Task<decimal> GetTotalRevenueAsync()
    {
        DateTime dataMin = DateTime.MinValue;
        var purchases = await _dal.FindListAsync(p => p.PurchaseDate >= dataMin);

        return purchases.Sum(p => p.PurchasePrice);
    }

    public async Task<decimal> GetRevenueInPeriodAsync(DateTime startDate, DateTime endDate)
    {
        DateTime dataMin = DateTime.MinValue;
        var purchases = await _dal.FindListAsync(p => p.PurchaseDate >= startDate && p.PurchaseDate <= endDate);

        return purchases.Sum(p => p.PurchasePrice);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}