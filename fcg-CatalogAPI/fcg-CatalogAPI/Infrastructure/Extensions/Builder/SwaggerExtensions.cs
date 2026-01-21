using Infrastructure.Swagger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Infrastructure.Extensions.Builder;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "FCG API",
                Version = "v1",
                Description = "API de Catálogo",
                Contact = new OpenApiContact
                {
                    Name = "FCG Team",
                    Email = "suporte@com"
                }
            });
            
          

            //c.SwaggerEndpoint("/swagger/v1/swagger.json", "ScreenSound API v1");
            //c.RoutePrefix = string.Empty;

            c.DocumentFilter<AlphabeticalDocumentFilter>();
            
        });

        return services;
    }
}
