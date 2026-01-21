using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid id);
    Task<Game?> GetByIdAsync(string EAN);
    Task<IEnumerable<Game>> GetAllAsync();
    Task<IEnumerable<Game>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<Game> AddAsync(Game game);
    Task UpdateAsync(Game game);
    Task DeleteAsync(Game game);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsAsync(string EAN);
    Task<bool> ExistsAsync(Guid id, string EAN);
    Task<int> SaveChangesAsync();
}