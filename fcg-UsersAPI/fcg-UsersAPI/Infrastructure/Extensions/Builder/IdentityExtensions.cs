// FCG.Infrastructure/Extensions/Builder/ConfigureIdentityExtensions.cs
using Domain.Data.Contexts;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Extensions.Builder;

public static class IdentityExtensions
{
    public static IHostApplicationBuilder AddIdentity(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DbFcg");

        builder.Services.AddDbContext<UserDbContext>(options =>
            options.UseNpgsql(connectionString));

        builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequiredUniqueChars = 1;
        })
        .AddEntityFrameworkStores<UserDbContext>()
        .AddErrorDescriber<PortugueseIdentityErrorDescriber>()
        .AddDefaultTokenProviders();

        return builder;
    }
}

#region traduçao dos critérios do identity
public class PortugueseIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError() => new()
    {
        Code = nameof(DefaultError),
        Description = "Ocorreu um erro desconhecido."
    };

    public override IdentityError ConcurrencyFailure() => new()
    {
        Code = nameof(ConcurrencyFailure),
        Description = "Falha de concorrência otimista. O objeto foi modificado."
    };

    public override IdentityError PasswordTooShort(int length) => new()
    {
        Code = nameof(PasswordTooShort),
        Description = $"A senha deve ter pelo menos {length} caracteres."
    };

    public override IdentityError PasswordRequiresNonAlphanumeric() => new()
    {
        Code = nameof(PasswordRequiresNonAlphanumeric),
        Description = "A senha deve conter pelo menos um caractere não alfanumérico."
    };

    public override IdentityError PasswordRequiresDigit() => new()
    {
        Code = nameof(PasswordRequiresDigit),
        Description = "A senha deve conter pelo menos um número ('0'-'9')."
    };

    public override IdentityError PasswordRequiresLower() => new()
    {
        Code = nameof(PasswordRequiresLower),
        Description = "A senha deve conter pelo menos uma letra minúscula ('a'-'z')."
    };

    public override IdentityError PasswordRequiresUpper() => new()
    {
        Code = nameof(PasswordRequiresUpper),
        Description = "A senha deve conter pelo menos uma letra maiúscula ('A'-'Z')."
    };

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => new()
    {
        Code = nameof(PasswordRequiresUniqueChars),
        Description = $"A senha deve conter pelo menos {uniqueChars} caracteres únicos."
    };

    public override IdentityError DuplicateEmail(string email) => new()
    {
        Code = nameof(DuplicateEmail),
        Description = $"O e-mail '{email}' já está em uso."
    };

    public override IdentityError DuplicateUserName(string userName) => new()
    {
        Code = nameof(DuplicateUserName),
        Description = $"O nome de usuário '{userName}' já está em uso."
    };
}
#endregion
