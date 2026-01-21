using Application.Interfaces.Repository;
using Domain.Data;
using Domain.Data.Contexts;
using Domain.Entities;

namespace Application.Repositories;

public class GameRepository : IGameRepository
{
    private readonly IDAL<Game> _dal;
    private readonly FcgDbContext _context;

    public GameRepository(IDAL<Game> dal, FcgDbContext context)
    {
        _dal = dal;
        _context = context;
    }

    public async Task<Game> AddAsync(Game game)
    {
        await _dal.AddAsync(game);
        await SaveChangesAsync();
        return game;
    }

    public async Task<Game?> GetByIdAsync(Guid id)
    {
        return await _dal.FindAsync(g => g.Id == id);
    }

    public async Task<Game?> GetByIdAsync(string EAN)
    {
        return await _dal.FindAsync(g => g.EAN == EAN);
    }

    public async Task<IEnumerable<Game>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var result = await _dal.FindListAsync(g => ids.Contains(g.Id));
        return result;
    }

    public async Task<IEnumerable<Game>> GetAllAsync()
    {
        return await _dal.ListAsync();
    }

    public async Task DeleteAsync(Game game)
    {
        await _dal.DeleteAsync(game);
        await SaveChangesAsync();
    }

    public async Task UpdateAsync(Game game)
    {
        await _dal.UpdateAsync(game);
        await SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var game = await _dal.FindAsync(g => g.Id == id);
        return game != null;
    }

    public async Task<bool> ExistsAsync(string EAN)
    {
        var game = await _dal.FindAsync(g => g.EAN == EAN);
        return game != null;
    }

    public async Task<bool> ExistsAsync(Guid id, string EAN)
    {
        var game = await _dal.FindAsync(g => g.Id == id && g.EAN == EAN);
        return game != null;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}