using Application.Dto.Filter;
using Application.Dto.Order;
using Application.Dto.Request;
using Application.Dto.Response;
using Application.Dto.Result;

namespace Application.Interfaces.Service;

public interface IGameService
{
    Task<OperationResult<GameResponseDto>> CreateGameAsync(GameCreateRequestDto dto);
    Task<OperationResult<GameResponseDto?>> GetGameByIdAsync(Guid id);
    Task<OperationResult<PagedResult<GameResponseDto>>> GetAllGamesAsync(PagedRequestDto<GameFilterDto, GameOrderDto> pagedRequestDto);
    Task<OperationResult<GameResponseDto>> UpdateGameAsync(Guid id, GameUpdateRequestDto dto);
    Task<OperationResult> DeleteGameAsync(Guid id);
    Task<bool> GameExistsAsync(Guid id);
    Task<bool> GameExistsAsync(string ean);
}