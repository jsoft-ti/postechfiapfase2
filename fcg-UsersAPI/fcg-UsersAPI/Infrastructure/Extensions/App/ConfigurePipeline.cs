using Domain.Enums;
using Infrastructure.Extensions.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Extensions.App;

public static class ConfigurePipelineExtensions
{
    public static async Task<WebApplication> ConfigurePipeline(this WebApplication app, ProjectType projectType)
    {
        // HSTS should be enabled in production
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        switch (projectType)
        {
            case ProjectType.Api:
                await app.ApplyMigrationsAsync();
                app.ConfigureApiPipeline();
                break;

            case ProjectType.Blazor:
                app.ConfigureBlazorPipeline();
                break;

            case ProjectType.Host:
                app.ConfigureHostPipeline();
                break;

            case ProjectType.Application:
                app.ConfigureApplicationPipeline();
                break;

            default:
                break;
        }

        return app;
    }

    private static void ConfigureApiPipeline(this WebApplication app)
    {
        // Configure global exception handler first in the pipeline
        app.UseExceptionHandler();

        //if (app.Environment.IsDevelopment())
        //{
            app.UseSwagger();
            app.UseSwaggerUI();
        //}

        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapDefaultEndpoints();

        app.MapControllers();
    }

    private static void ConfigureBlazorPipeline(this WebApplication app)
    {
        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseAntiforgery();

        app.UseOutputCache();
    }

    private static void ConfigureHostPipeline(this WebApplication app)
    {
        // Configurações específicas do Host, se houver
        app.MapHealthChecksEndpoints();
    }

    private static void ConfigureApplicationPipeline(this WebApplication app)
    {
        // Configurações específicas do Application, se houver
        app.MapHealthChecksEndpoints();
    }

    private static void MapHealthChecksEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapHealthChecks("/health");

            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }
    }
}
