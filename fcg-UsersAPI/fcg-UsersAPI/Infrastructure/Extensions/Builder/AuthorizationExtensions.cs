using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Extensions.Builder;

public static class AuthorizationPolicyExtensions
{
    public static IHostApplicationBuilder AddAuthorizationPolicies(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(options =>
        {
            // Apenas usuários com role "Admin" podem cadastrar jogos
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin"));

            // Apenas usuários com role "Player" podem adicionar jogos à biblioteca
            options.AddPolicy("PlayerOnly", policy =>
                policy.RequireRole("Player"));

            // Qualquer usuário autenticado pode visualizar jogos
            options.AddPolicy("AuthenticatedUser", policy =>
                policy.RequireAuthenticatedUser());

            // Exemplo de política baseada em claim personalizada
            options.AddPolicy("CanManageLibrary", policy =>
                policy.RequireClaim("Permissao", "GerenciarBiblioteca"));
        });

        return builder;
    }
}
