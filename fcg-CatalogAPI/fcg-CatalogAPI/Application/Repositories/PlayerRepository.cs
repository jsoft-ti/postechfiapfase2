using Application.Interfaces.Repository;
using Domain.Data;
using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly IDAL<Player> _dal;

    public PlayerRepository(IDAL<Player> dal)
    {
        _dal = dal;
    }

    public async Task AddAsync(Player player)
    {
        if (player == null)
            throw new ArgumentNullException(nameof(player));

        await _dal.AddAsync(player);
    }

    public async Task<IEnumerable<Player>> GetAllAsync()
    {
        Expression<Func<Player, object>> libraryInclude = p => p.Library;
        var players = await _dal.ListAsync(libraryInclude);
        return players;
    }

    public async Task<Player?> GetByIdAsync(Guid id)
    {
        Expression<Func<Player, object>> libraryInclude = p => p.Library;
        Expression<Func<Player, bool>> idPredicate = p => p.Id == id;
        return await _dal.FindAsync(idPredicate, libraryInclude);
    }

    public async Task<Player?> GetByUserIdAsync(Guid userId)
    {
        Expression<Func<Player, object>> libraryInclude = p => p.Library;
        Expression<Func<Player, bool>> userIdPredicate = p => p.UserId == userId;
        return await _dal.FindAsync(userIdPredicate, libraryInclude);
    }

    public async Task UpdateAsync(Player player)
    {
        if (player == null)
            throw new ArgumentNullException(nameof(player));

        // Check if new DisplayName would create a duplicate
        Expression<Func<Player, bool>> displayNamePredicate = p => p.DisplayName == player.DisplayName && p.Id != player.Id;
        var existing = await _dal.FindAsync(displayNamePredicate);

        if (existing != null)
            throw new InvalidOperationException("DisplayName já está em uso.");

        await _dal.UpdateAsync(player);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        Expression<Func<Player, bool>> idPredicate = p => p.Id == id;
        var player = await _dal.FindAsync(idPredicate);
        return player != null;
    }

    public async Task<bool> ExistsByUserIdAsync(Guid userId)
    {
        Expression<Func<Player, bool>> userIdPredicate = p => p.UserId == userId;
        var player = await _dal.FindAsync(userIdPredicate);
        return player != null;
    }

    public async Task<bool> ExistsByDisplayNameAsync(string displayName)
    {
        Expression<Func<Player, bool>> displayNamePredicate = p => p.DisplayName == displayName;
        var player = await _dal.FindAsync(displayNamePredicate);
        return player != null;
    }
}