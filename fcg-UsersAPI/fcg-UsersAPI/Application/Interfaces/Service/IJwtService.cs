using Domain.Entities;
using System.Security.Claims;

namespace Application.Interfaces.Service;

public interface IJwtService
{
    Task<string> GenerateToken(User userIdentity);
    Task<string> GenerateRefreshToken(User userIdentity);
    Task<bool> ValidateRefreshToken(User userIdentity, string refreshToken);
    ClaimsPrincipal? ValidateToken(string token);
}

