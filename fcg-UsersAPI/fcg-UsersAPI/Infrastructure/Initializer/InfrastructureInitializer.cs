using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Infrastructure.Interfaces;
using Domain.Data.Contexts;
using Microsoft.Extensions.Configuration;
using Domain.Data.Contexts.Extension;

namespace Infrastructure.Initializer;

public class InfrastructureInitializer : IInfrastructureInitializer
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<InfrastructureInitializer> _logger;
    private readonly IConfiguration _configuration;

    public InfrastructureInitializer(IServiceProvider provider, ILogger<InfrastructureInitializer> logger, IConfiguration configuration)
    {
        _provider = provider;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Infraestrutura inicializando seeds.");

            // 1. Identity
            using (var scope = _provider.CreateScope())
            {
                var sp = scope.ServiceProvider;
                var identityDbContext = sp.GetRequiredService<UserDbContext>();
                //var seedService = sp.GetRequiredService<ISeedService>();

                var dropDatabase = _configuration.GetValue<bool>("db_delete:database");

#if DEBUG
                if (dropDatabase)
                    identityDbContext.Database.EnsureDeleted();
#endif

                // Aplica apenas migrations pendentes do Identity
                var pendingMigrations = await identityDbContext.Database.GetPendingMigrationsAsync(cancellationToken);
                if (pendingMigrations.Any())
                {
#if DEBUG
                    var dropSchema = _configuration.GetValue<bool>("db_delete:schema:identity");

                    if (dropSchema)
                        await identityDbContext.DropSchemaAsync(_logger, cancellationToken);
#endif

                    _logger.LogInformation("Aplicando migrations do Identity...");
                    await identityDbContext.Database.MigrateAsync(cancellationToken);
                    _logger.LogInformation("AS migrations do Identity aplicadas ...");
                }
                
                //await seedService.SeedIdentityAsync(sp, cancellationToken);
            }

            _logger.LogInformation("Infraestrutura e seeds inicializados com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao inicializar infraestrutura (migrations/seeds)");
            throw;
        }
    }
}