using Domain.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Domain.Data
{
    public class IdentityDAL<T> : IDAL<T> where T : class
    {
        private readonly UserDbContext _context;
        private readonly ILogger<IdentityDAL<T>> _logger;

        public IdentityDAL(UserDbContext context, ILogger<IdentityDAL<T>> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddAsync(T item)
        {
            _logger.LogInformation("Adicionando entidade do tipo {EntityType}", typeof(T).Name);
            await _context.Set<T>().AddAsync(item);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Entidade adicionada com sucesso: {Entity}", item);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao adicionar entidade {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task UpdateAsync(T item)
        {
            _logger.LogInformation("Atualizando entidade do tipo {EntityType}", typeof(T).Name);
            _context.Entry(item).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Entidade atualizada com sucesso: {Entity}", item);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao atualizar entidade {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task DeleteAsync(T item)
        {
            _logger.LogInformation("Removendo entidade do tipo {EntityType}", typeof(T).Name);
            _context.Set<T>().Remove(item);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Entidade removida com sucesso: {Entity}", item);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao remover entidade {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task<T?> FindAsync(Expression<Func<T, bool>> condicao, params Expression<Func<T, object>>[] includes)
        {
            _logger.LogDebug("Buscando entidade do tipo {EntityType} com condição {Condition}", typeof(T).Name, condicao);
            IQueryable<T> query = _context.Set<T>().Where(condicao);
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            var result = await query.FirstOrDefaultAsync();
            _logger.LogInformation("Busca concluída. Resultado encontrado: {Found}", result != null);
            return result;
        }

        public async Task<List<T>> ListAsync(params Expression<Func<T, object>>[] includes)
        {
            _logger.LogDebug("Buscando entidade do tipo {EntityType} com condição {Includes}", typeof(T).Name, includes);
            IQueryable<T> query = _context.Set<T>();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            var result = await query.ToListAsync();
            _logger.LogInformation("Busca concluída. Resultado encontrado: {Found}", result.Count());
            return result;
        }

        public async Task<List<T>> FindListAsync(Expression<Func<T, bool>> condicao, params Expression<Func<T, object>>[] includes)
        {
            _logger.LogDebug("Buscando lista de entidades do tipo {EntityType} com condição {Condition}", typeof(T).Name, condicao);

            IQueryable<T> query = _context.Set<T>().Where(condicao);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            var result = await query.ToListAsync();
            _logger.LogInformation("Busca concluída. Total encontrado: {Count}", result.Count);
            return result;
        }

        public IQueryable<T> Query(params Expression<Func<T, object>>[] includes)
        {
            _logger.LogDebug("Expondo IQueryable para entidade do tipo {EntityType}", typeof(T).Name);
            IQueryable<T> query = _context.Set<T>();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return query.AsQueryable();
        }

        // ✅ Método novo
        public DbContext GetDbContext()
        {
            return _context;
        }
    }
}