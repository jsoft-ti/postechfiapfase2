using Application.Dto.Filter;
using Application.Dto.Order;
using Application.Dto.Request;
using Application.Dto.Response;
using Application.Dto.Result;
using Application.Interfaces;
using Application.Security;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;

    public UserService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<OperationResult<PagedResult<UserInfoResponseDto>>> GetAllAsync(PagedRequestDto<UserFilterDto, UserOrderDto> dto)
    {
        var users = await GetUsers(dto);
        var userDtos = users.Select(Map).ToList();
        var totalItems = userDtos.Count;

        var pagedList = new PagedResult<UserInfoResponseDto>
        {
            Items = userDtos,
            PageNumber = dto.PageNumber,
            PageSize = dto.PageSize,
            TotalItems = totalItems
        };

        return OperationResult<PagedResult<UserInfoResponseDto>>.Success(pagedList);
    }

    public async Task<OperationResult<PagedResult<UserInfoResponseDto>>> GetUsersByRoleAsync(Roles role, PagedRequestDto<UserFilterDto, UserOrderDto> dto)
    {
        var users = await GetUsers(dto);

        // Cria uma lista de tarefas para buscar roles de todos os usuários
        var roleTasks = users.Select(async user => new
        {
            User = user,
            Roles = await _userManager.GetRolesAsync(user)
        });

        var roleName = role.ToString();
        var usersWithRoles = await Task.WhenAll(roleTasks);

        // Filtra os usuários com base no papel
        var filteredUsers = usersWithRoles
            .Where(x => x.Roles.Contains(roleName))
            .Select(x => Map(x.User))
            .ToList();

        var totalItems = filteredUsers.Count;

        var pagedList = new PagedResult<UserInfoResponseDto>
        {
            Items = filteredUsers,
            PageNumber = dto.PageNumber,
            PageSize = dto.PageSize,
            TotalItems = totalItems
        };

        return OperationResult<PagedResult<UserInfoResponseDto>>.Success(pagedList);
    }

    public async Task<OperationResult<UserInfoResponseDto?>> GetByIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return OperationResult<UserInfoResponseDto?>.Failure("Usuário não encontrado.");

        var roles = await _userManager.GetRolesAsync(user);

        var userMap = roles.Contains(RoleConstants.Player) ? Map(user) : null;

        if (userMap == null) return OperationResult<UserInfoResponseDto?>.Failure("Usuário não encontrado.");

        return OperationResult<UserInfoResponseDto?>.Success(Map(user));
    }

    public async Task<OperationResult> UpdateUserAsync(Guid userId, UserUpdateRequestDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return OperationResult.Failure("Usuário não encontrado.");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.DisplayName = dto.DisplayName;
        user.Email = dto.Email;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded ? OperationResult.Success() : OperationResult.Failure("Erro ao atualizar usuário.");
    }

    public async Task<OperationResult> UpdatePasswordAsync(Guid userId, UserUpdateRequestDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return OperationResult.Failure("Usuário não encontrado.");

        if (dto.NewPassword != dto.ConfirmNewPassword)
            return OperationResult.Failure("Nova senha e confirmação não conferem.");

        var result = await _userManager.ChangePasswordAsync(user, dto.Password, dto.NewPassword);
        if (!result.Succeeded)
            return OperationResult.Failure(result.Errors.Select(e => e.Description).ToArray());

        return OperationResult.Success();
    }

    public async Task<OperationResult> DeleteUserAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return OperationResult.Failure("Usuário não encontrado.");

        user.IsActive = false;
        var result = await _userManager.UpdateAsync(user);

        return result.Succeeded ? OperationResult.Success() : OperationResult.Failure("Erro ao inativar usuário.");
    }

    #region privates methods
    private async Task<List<User>> GetUsers(PagedRequestDto<UserFilterDto, UserOrderDto> dto)
    {
        var query = _userManager.Users.Where(u => u.EmailConfirmed);

        // ✅ Aplicar filtros
        if (dto.Filter != null)
        {
            if (!string.IsNullOrWhiteSpace(dto.Filter.Name))
                query = query.Where(u => u.DisplayName.Contains(dto.Filter.Name));

            if (!string.IsNullOrWhiteSpace(dto.Filter.Email))
                query = query.Where(u => u.Email != null && u.Email.Contains(dto.Filter.Email));
        }

        // ✅ Ordenação dinâmica com TOrder
        if (dto.OrderBy != null)
        {
            query = dto.OrderBy.SortBy.ToLower() switch
            {
                "firstname" => dto.OrderBy.Ascending ? query.OrderBy(u => u.FirstName) : query.OrderByDescending(u => u.FirstName),
                "lastname" => dto.OrderBy.Ascending ? query.OrderBy(u => u.LastName) : query.OrderByDescending(u => u.LastName),
                "email" => dto.OrderBy.Ascending ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
                _ => dto.OrderBy.Ascending ? query.OrderBy(u => u.DisplayName) : query.OrderByDescending(u => u.DisplayName)
            };
        }

        var users = await query
            .Skip((dto.PageNumber - 1) * dto.PageSize)
            .Take(dto.PageSize)
            .ToListAsync();

        return users;
    }

    private UserInfoResponseDto Map(User user) => new UserInfoResponseDto
    {
        Id = user.Id,
        DisplayName = user.DisplayName,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email!
    };
    #endregion
}