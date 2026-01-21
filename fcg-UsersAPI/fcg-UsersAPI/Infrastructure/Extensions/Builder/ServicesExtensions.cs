using Application.Interfaces;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Application.Repositories;
using Application.Services;
using Domain.Data;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Extensions.Builder;
//using Domain.Service;
using Infrastructure.Initializer;
using Infrastructure.Interfaces;
using Infrastructure.MessageBus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Infrastructure.Extensions.Builder;

public static class ServicesExtensions
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder, ProjectType projectType)
    {
        switch (projectType)
        {
            case ProjectType.Api:
                builder.ConfigureApiServices();
                break;

            case ProjectType.Blazor:
                builder.ConfigureBlazorServices();
                break;

            case ProjectType.Host:
                builder.ConfigureHostServices();
                break;

            case ProjectType.Application:
                builder.ConfigureApplicationServices();
                break;

            default:
                break;
        }

        builder.AddServiceDefaults(projectType);

        return builder;
    }

    private static IHostApplicationBuilder ConfigureApiServices(this IHostApplicationBuilder builder)
    {
        builder.AddDatabase();
        builder.AddIdentity();
        builder.AddAuthorizationJWT();
        builder.AddAuthorizationPolicies();

        builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

        builder.Services.AddScoped(typeof(IdentityDAL<>)); // para identidade
        //builder.Services.AddScoped(typeof(IDAL<>), typeof(DomainDAL<>)); // padrão para domínio
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        //builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
        //builder.Services.AddScoped<IGameRepository, GameRepository>();
        //builder.Services.AddScoped<IGalleryRepository, GalleryRepository>();
        //builder.Services.AddScoped<ILibraryRepository, LibraryRepository>();
       // builder.Services.AddScoped<ICartRepository, CartRepository>();
        //builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();
        //builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();

        //builder.Services.AddScoped<ICartDomainService, CartDomainService>();
        //builder.Services.AddScoped<IPurchaseDomainService, PurchaseDomainService>();

        //builder.Services.AddScoped<ISeedService, SeedService>();
        builder.Services.AddScoped<IInfrastructureInitializer, InfrastructureInitializer>();
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        //builder.Services.AddScoped<IPlayerService, PlayerService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddSingleton<IMessageBusProducer, RabbitMqProducer>();
        builder.Services.AddSingleton<IMessageService, UserCreatedEventService>();
        
        
        //builder.Services.AddScoped<IGameService, GameService>();
        //builder.Services.AddScoped<IGalleryService, GalleryService>();
        //builder.Services.AddScoped<ICartService, CartService>();
        //builder.Services.AddScoped<ILibraryService, LibraryService>();
        //builder.Services.AddScoped<IPurchaseService, PurchaseService>();


        builder.Services.AddControllers();
        builder.Services.AddExceptionHandler();
        builder.Services.AddSwaggerDocumentation();

        builder.Services.ConfigureHttpClientDefaults(static http =>
        {
            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });
        //Registrando o serviço do RabbitMQ que foi criado no Messagin
        //builder.Services.AddSingleton<IMessageBusProducer, RabbitMqProducer>();
        
        return builder;
    }

    private static IHostApplicationBuilder ConfigureBlazorServices(this IHostApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddOutputCache();

        return builder;
    }

    private static IHostApplicationBuilder ConfigureHostServices(this IHostApplicationBuilder builder)
    {
        // Configurações específicas do Host, se houver
        return builder;
    }

    private static IHostApplicationBuilder ConfigureApplicationServices(this IHostApplicationBuilder builder)
    {
        // Configurações específicas do Application, se houver
        return builder;
    }

    private static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder, ProjectType projectType)
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                    //.AddGrpcClientInstrumentation() // se usar gRPC
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Exemplo para Azure Monitor (descomente se usar)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
    }

    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), new[] { "live" });

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapHealthChecks("/health");

            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }
}
