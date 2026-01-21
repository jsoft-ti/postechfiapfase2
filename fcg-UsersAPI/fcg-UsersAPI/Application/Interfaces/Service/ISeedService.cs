using Domain.Data.Contexts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

public interface ISeedService
{
    /// <summary>
    /// Executa o seed apenas dos dados do Identity (roles, admin e players).
    /// Precisa do IServiceProvider para acessar UserManager/RoleManager.
    /// </summary>
    Task SeedIdentityAsync(IServiceProvider sp, CancellationToken cancellationToken);

    /// <summary>
    /// Executa o seed apenas dos dados da aplicação (jogos, players, carrinhos, biblioteca, etc.).
    /// </summary>
    Task SeedApplicationAsync(IServiceProvider sp, CancellationToken cancellationToken);
}
