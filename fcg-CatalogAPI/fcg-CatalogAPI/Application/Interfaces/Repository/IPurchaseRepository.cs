namespace Application.Interfaces.Repository;

public interface IPurchaseRepository
{
    Task<IEnumerable<Purchase>> GetByPlayerAsync(Guid playerId);
    Task<IEnumerable<Purchase>> GetAllAsync();
    Task AddAsync(Purchase purchase);
    IQueryable<Purchase> Query();
}
