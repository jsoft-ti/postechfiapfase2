using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Domain.Data;

public interface IDAL<T> where T : class
{
    Task AddAsync(T item);
    Task UpdateAsync(T item);
    Task DeleteAsync(T item);
    Task<T?> FindAsync(Expression<Func<T, bool>> condicao, params Expression<Func<T, object>>[] includes);
    Task<List<T>> ListAsync(params Expression<Func<T, object>>[] includes);
    Task<List<T>> FindListAsync(Expression<Func<T, bool>> condicao, params Expression<Func<T, object>>[] includes);
    IQueryable<T> Query(params Expression<Func<T, object>>[] includes);
    DbContext GetDbContext();
}

