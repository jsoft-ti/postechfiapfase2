using Application.Dto.Response;
using Application.Interfaces.Service;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Application.Dto.Result;
using Application.Dto.Request;
using Application.Security;
using Domain.Enums;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtService _jwtService;
    //private readonly IPlayerService _playerService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(UserManager<User> userManager,
                       IJwtService jwtService,
                       //IPlayerService playerService,
                       ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        //_playerService = playerService;
        _logger = logger;
    }

    /// <summary>
    /// Registra um novo usuário e atribui uma role.
    /// Se a role for Player, também cria o registro na tabela Players.
    /// </summary>
    public async Task<OperationResult<User>> RegisterUserAsync(UserCreateRequestDto dto, string role)
    {
        var info = new List<string>();

        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DisplayName = dto.DisplayName
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        LogRegistrationAttempt(dto.Email, result.Succeeded);

        if (!result.Succeeded)
            return OperationResult<User>.Failure(result.Errors.Select(e => e.Description).ToArray());

        info.Add($"User: {dto.Email} foi registrado com sucesso com o Id: {user.Id}");

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            // Se falhar ao adicionar role, remove o usuário criado
            await _userManager.DeleteAsync(user);
            return OperationResult<User>.Failure(roleResult.Errors.Select(e => e.Description).ToArray());
        }

        info.Add($", foi atribuido a role: {role}");

        _logger.LogInformation(info.ToString());

        return OperationResult<User>.Success(user);
    }

    /// <summary>
    /// Realiza login e gera token JWT.
    /// </summary>
    public async Task<OperationResult<UserAuthResponseDto>> LoginAsync(UserLoginRequestDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            LogLoginAttempt(dto.Email, false);
            return OperationResult<UserAuthResponseDto>.Failure("Credenciais inválidas.");
        }

        LogLoginAttempt(dto.Email, true);
        var tokenData = await CreateToken(user);
        return OperationResult<UserAuthResponseDto>.Success(tokenData);
    }

    /// <summary>
    /// Atualiza token JWT usando refresh token.
    /// </summary>
    public async Task<OperationResult<UserAuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto dto)
    {
        var principal = _jwtService.ValidateToken(dto.Token);
        if (principal == null)
            return OperationResult<UserAuthResponseDto>.Failure("Token inválido.");

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return OperationResult<UserAuthResponseDto>.Failure("Token inválido.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !await _jwtService.ValidateRefreshToken(user, dto.RefreshToken))
            return OperationResult<UserAuthResponseDto>.Failure("Refresh token inválido.");

        var tokenData = await CreateToken(user);
        return OperationResult<UserAuthResponseDto>.Success(tokenData);
    }

    /// <summary>
    /// Obtém informações do usuário atual.
    /// </summary>
    public async Task<OperationResult<UserInfoResponseDto>> GetCurrentUserAsync(ClaimsPrincipal userPrincipal)
    {
        var userId = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _userManager.FindByIdAsync(userId!);

        if (user == null)
            return OperationResult<UserInfoResponseDto>.Failure("Usuário não encontrado.");

        return OperationResult<UserInfoResponseDto>.Success(GetUserInfo(user));
    }

    /// <summary>
    /// Atualiza senha do usuário.
    /// </summary>
    public async Task<OperationResult> UpdatePasswordAsync(Guid userId, UserUpdateRequestDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return OperationResult.Failure("Usuário não encontrado.");

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
            return OperationResult.Failure("Senha atual incorreta.");

        var result = await _userManager.ChangePasswordAsync(user, dto.Password, dto.NewPassword);
        if (!result.Succeeded)
            return OperationResult.Failure(result.Errors.Select(e => e.Description).ToArray());

        return OperationResult.Success();
    }

    /// <summary>
    /// Cria token JWT e refresh token.
    /// </summary>
    private async Task<UserAuthResponseDto> CreateToken(User user)
    {
        var token = await _jwtService.GenerateToken(user);
        var refreshToken = await _jwtService.GenerateRefreshToken(user);

        return new UserAuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = GetUserInfo(user)
        };
    }

    /// <summary>
    /// Monta DTO com informações do usuário.
    /// </summary>
    private UserInfoResponseDto GetUserInfo(User user)
    {
        return new UserInfoResponseDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email!,
        };
    }

    private void LogLoginAttempt(string email, bool success)
    {
        if (success)
            _logger.LogInformation("Login bem-sucedido para o usuário: {Email}", email);
        else
            _logger.LogWarning("Tentativa de login falhou para o usuário: {Email}", email);
    }

    private void LogRegistrationAttempt(string email, bool success)
    {
        if (success)
            _logger.LogInformation("Registro bem-sucedido para o usuário: {Email}", email);
        else
            _logger.LogWarning("Tentativa de registro falhou para o usuário: {Email}", email);
    }
}
