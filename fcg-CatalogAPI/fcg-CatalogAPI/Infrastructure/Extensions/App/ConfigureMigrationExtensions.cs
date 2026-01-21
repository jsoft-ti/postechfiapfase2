using Domain.Data.Contexts;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Extensions.App;

public static class ConfigureMigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("Migration");

        try
        {
            var initializer = sp.GetRequiredService<IInfrastructureInitializer>();
            await initializer.InitializeAsync();
            logger.LogInformation("Migrations e seeds aplicadas com sucesso");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao aplicar migrations/seeds");
            throw;
        }
    }
}
