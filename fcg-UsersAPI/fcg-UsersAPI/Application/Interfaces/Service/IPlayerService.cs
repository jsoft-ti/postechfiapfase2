using Application.Dto.Request;
using Application.Dto.Response;
using Application.Dto.Result;
using Domain.Entities;

namespace Application.Interfaces.Service;

public interface IPlayerService
{
    Task<OperationResult<PlayerWithUserResponseDto>> GetByIdAsync(Guid playerId);
    Task<OperationResult<PlayerWithUserResponseDto>> GetByUserIdAsync(Guid userId);
    Task<OperationResult> CreateAsync(PlayerCreateDto dto);
    Task<OperationResult> UpdateDisplayNameAsync(Guid playerId, string newDisplayName);
    Task<OperationResult<IEnumerable<PlayerResponseDto>>> GetAllAsync();
    Task<bool> ExistsAsync(Guid playerId);
    Task<bool> ExistsByUserIdAsync(Guid userId);
}