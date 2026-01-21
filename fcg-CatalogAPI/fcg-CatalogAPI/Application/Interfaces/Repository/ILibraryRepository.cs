using Domain.Entities;

namespace Application.Interfaces.Repository;

public interface ILibraryRepository
{
    // Library management
    Task<LibraryGame?> GetLibraryGameAsync(Guid playerId, Guid gameId);
    Task<IEnumerable<LibraryGame>> GetPlayerLibraryAsync(Guid playerId);
    Task<(bool, LibraryGame)> AddToLibraryAsync(LibraryGame game);

    // Ownership and verification
    Task<bool> OwnsLibraryGameAsync(Guid playerId, Guid gameId);
    Task<bool> HasGameInLibraryAsync(Guid playerId, string gameName);
    
    // Library queries
    Task<IEnumerable<LibraryGame>> GetRecentPurchasesAsync(Guid playerId, int count);
    Task<IEnumerable<LibraryGame>> GetGamesByGenreAsync(Guid playerId, string genre);
    Task<decimal> GetTotalSpentAsync(Guid playerId);
    Task<int> GetLibraryGameCountAsync(Guid playerId);
    
    // Purchase history
    Task<IEnumerable<LibraryGame>> GetPurchasesByDateRangeAsync(Guid playerId, DateTime startDate, DateTime endDate);
    Task<decimal> GetSpentInPeriodAsync(Guid playerId, DateTime startDate, DateTime endDate);
    
    // Admin queries
    Task<IEnumerable<LibraryGame>> GetAllPurchasesAsync();
    IQueryable<LibraryGame> GetAllPurchasesQueryable();
    Task<int> GetTotalPurchaseCountAsync();
    Task<decimal> GetTotalRevenueAsync();
    Task<decimal> GetRevenueInPeriodAsync(DateTime startDate, DateTime endDate);
    
    Task<int> SaveChangesAsync();
}