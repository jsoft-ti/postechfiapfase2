using Domain.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Domain.Data.Factories
{
    public class DesignTimeFcgDbContextFactory : IDesignTimeDbContextFactory<FcgDbContext>
    {
        public FcgDbContext CreateDbContext(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../fcg-CatalogAPI");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DbFcg");

            var optionsBuilder = new DbContextOptionsBuilder<FcgDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new FcgDbContext(optionsBuilder.Options);
        }
    }
}
