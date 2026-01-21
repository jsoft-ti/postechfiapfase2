using Application.Dto.Response;
using Domain.Entities;
using System.Security.Claims;
using Application.Dto.Result;
using Application.Dto.Request;

namespace Application.Interfaces.Service;

public interface IAuthService
{
    Task<OperationResult<User>> RegisterUserAsync(UserCreateRequestDto dto, string role);
    Task<OperationResult<UserAuthResponseDto>> LoginAsync(UserLoginRequestDto dto);
    Task<OperationResult<UserAuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto dto);
    Task<OperationResult<UserInfoResponseDto>> GetCurrentUserAsync(ClaimsPrincipal userPrincipal);
    Task<OperationResult> UpdatePasswordAsync(Guid userId, UserUpdateRequestDto dto);
}