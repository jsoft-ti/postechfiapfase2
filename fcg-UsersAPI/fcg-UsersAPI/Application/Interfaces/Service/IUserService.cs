using Application.Dto.Filter;
using Application.Dto.Order;
using Application.Dto.Request;
using Application.Dto.Response;
using Application.Dto.Result;
using Domain.Enums;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<OperationResult<PagedResult<UserInfoResponseDto>>> GetAllAsync(PagedRequestDto<UserFilterDto, UserOrderDto> dto);
        Task<OperationResult<PagedResult<UserInfoResponseDto>>> GetUsersByRoleAsync(Roles role, PagedRequestDto<UserFilterDto, UserOrderDto> dto);
        Task<OperationResult<UserInfoResponseDto?>> GetByIdAsync(Guid userId);
        Task<OperationResult> UpdateUserAsync(Guid userId, UserUpdateRequestDto dto);
        Task<OperationResult> UpdatePasswordAsync(Guid userId, UserUpdateRequestDto dto);
        Task<OperationResult> DeleteUserAsync(Guid id);
    }
}