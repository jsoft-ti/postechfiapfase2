using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Infrastructure.Extensions.Builder;

public static class AuthenticationExtensions
{
    public static IHostApplicationBuilder AddAuthorizationJWT(this IHostApplicationBuilder builder)
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");

        if (string.IsNullOrEmpty(jwtSettings["SecretKey"]))
            throw new InvalidOperationException("JWT SecretKey não configurado.");

        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // true em produção
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                ClockSkew = TimeSpan.Zero
            };

            // Evento simplificado - sem modificar headers
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    // Apenas log ou outras ações que não modifiquem headers
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        // O cliente saberá que o token expirou pelo status 401
                    }
                    return Task.CompletedTask;
                }
            };
        });

        return builder;
    }
}
