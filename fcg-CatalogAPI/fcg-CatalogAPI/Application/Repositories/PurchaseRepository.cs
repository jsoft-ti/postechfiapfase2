using Application.Interfaces.Repository;
using Domain.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Repositories
{
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly IDAL<Purchase> _dal;

        public PurchaseRepository(IDAL<Purchase> dal)
        {
            _dal = dal;
        }

        public async Task AddAsync(Purchase purchase)
        {
            await _dal.AddAsync(purchase);
        }

        public async Task<IEnumerable<Purchase>> GetByPlayerAsync(Guid playerId)
        {
            return await _dal.FindListAsync(p => p.PlayerId == playerId, p => p.Game, p => p.Player);
        }

        public async Task<IEnumerable<Purchase>> GetAllAsync()
        {
            return await _dal.ListAsync(p => p.Game, p => p.Player);
        }

        public IQueryable<Purchase> Query()
        {
            return _dal.Query().Include(p => p.Game).Include(p => p.Player);
        }
    }
}
