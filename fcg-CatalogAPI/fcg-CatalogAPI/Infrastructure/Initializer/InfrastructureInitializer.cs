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

         
            using (var scope = _provider.CreateScope())
            {
                var sp = scope.ServiceProvider;
                var dropDatabase = _configuration.GetValue<bool>("db_delete:database");
                // 1. Aplicação
                var fcgDbContext = sp.GetRequiredService<FcgDbContext>();

                // Apenas garante que o banco existe e aplica migrations pendentes
                var appPendingMigrations = await fcgDbContext.Database.GetPendingMigrationsAsync(cancellationToken);
                if (appPendingMigrations.Any())
                {
#if DEBUG
                    var limparSchema = _configuration.GetValue<bool>("db_delete:schema:fcg");

                    if (limparSchema)
                        await fcgDbContext.DropSchemaAsync(_logger, cancellationToken);
#endif

                    _logger.LogInformation("Aplicando migrations da aplicação...");
                    await fcgDbContext.Database.MigrateAsync(cancellationToken);
                    _logger.LogInformation("AS migrations do Aplicação aplicadas ...");
                }

                //await seedService.SeedApplicationAsync(sp, cancellationToken);
            }

            _logger.LogInformation("Infraestrutura e seeds inicializados com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao inicializar infraestrutura (migrations/seeds)" + ex.Message);
            throw;
        }
    }
}