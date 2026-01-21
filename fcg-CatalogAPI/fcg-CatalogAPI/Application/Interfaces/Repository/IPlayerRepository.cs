using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(Guid id);
    Task<IEnumerable<Player>> GetAllAsync();
    Task AddAsync(Player player);
    Task UpdateAsync(Player player);
    Task<Player?> GetByUserIdAsync(Guid userId);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsByUserIdAsync(Guid userId);
    Task<bool> ExistsByDisplayNameAsync(string displayName);
}