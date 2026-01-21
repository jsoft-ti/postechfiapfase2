using Application.Dto.Request;
using Application.Dto.Response;
using Application.Dto.Result;

namespace Application.Interfaces.Service;

public interface ICartService
{
    Task<OperationResult> AddItemAsync(CartItemRequestDto request);
    Task<OperationResult> RemoveItemAsync(CartItemRequestDto request);
    Task<OperationResult<CartResponseDto>> GetCartByPlayerIdAsync(Guid playerId);
    Task<OperationResult<CartResponseDto>> GetCartByUserIdAsync(Guid userId);
    Task<OperationResult> ClearCartAsync(Guid playerId);
}