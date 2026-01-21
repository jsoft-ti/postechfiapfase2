using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Domain.Data.Contexts.Extension;

public static class DbContextExtension
{
    public static async Task DropSchemaAsync(this DbContext context, ILogger logger, CancellationToken cancellationToken)
    {
#if DEBUG
        string contextName = context.GetType().Name;
        var databaseName = context.Database.GetDbConnection().Database;
        string schema = contextName switch
        {
            "FcgDbContext" => "fcg",
            "UserDbContext" => "identity",
            _ => "public" // fallback
        };

        if (schema.Equals("public"))
            return;

        try
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            logger.LogInformation("Tentando remover schema '{Schema}' do banco '{Database}'...", schema, databaseName);

            var schemaName = schema.Replace("\"", "\"\"");
            var sql = $"DROP SCHEMA IF EXISTS \"{schemaName}\" CASCADE;";
            await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);

            logger.LogInformation("Schema '{Schema}' do banco '{Database}' removido com sucesso.", schema, databaseName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao remover schema '{Schema}' do banco {database}.",schema, databaseName);
            // opcional: relançar se quiser que a inicialização pare
            // throw;
        }
#else
            
#endif
    }    
}
