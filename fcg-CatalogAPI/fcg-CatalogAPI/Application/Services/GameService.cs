using Application.Dto.Filter;
using Application.Dto.Order;
using Application.Dto.Request;
using Application.Dto.Response;
using Application.Dto.Result;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<GameService> _logger;

    public GameService(IGameRepository gameRepository, ILogger<GameService> logger)
    {
        _gameRepository = gameRepository;
        _logger = logger;
    }

    public async Task<OperationResult<GameResponseDto>> CreateGameAsync(GameCreateRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.EAN))
            return OperationResult<GameResponseDto>.Failure("O EAN do jogo é obrigatório.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            return OperationResult<GameResponseDto>.Failure("O nome do jogo é obrigatório.");

        if (string.IsNullOrWhiteSpace(dto.Genre))
            return OperationResult<GameResponseDto>.Failure("O gênero do jogo é obrigatório.");

        var exist = await _gameRepository.ExistsAsync(dto.EAN);

        if (exist)
        {
            return OperationResult<GameResponseDto>.Failure("O EAN do jogo já existe no cadastro.");
        }

        var game = new Game(dto.EAN, dto.Title, dto.Genre, dto.Description, dto.Price);
        await _gameRepository.AddAsync(game);

        _logger.LogInformation("Jogo criado com sucesso: {GameId}", game.Id);

        return OperationResult<GameResponseDto>.Success(MapToResponse(game));
    }

    public async Task<OperationResult<GameResponseDto>> UpdateGameAsync(Guid id, GameUpdateRequestDto dto)
    {
        var game = await _gameRepository.GetByIdAsync(id);

        if (game == null)
            return OperationResult<GameResponseDto>.Failure("Jogo não encontrado.");

        if (!string.IsNullOrWhiteSpace(dto.EAN))
        {
            var exist = await _gameRepository.ExistsAsync(id, dto.EAN);

            if (exist)
                return OperationResult<GameResponseDto>.Failure("O EAN do jogo já existe no cadastro.");
        }

        game.Update(dto.EAN, dto.Title, dto.Genre, dto.Description,dto.Price ,dto.SubTitle );

        await _gameRepository.UpdateAsync(game);
        _logger.LogInformation("Jogo atualizado com sucesso: {GameId}", game.Id);

        return OperationResult<GameResponseDto>.Success(MapToResponse(game));
    }

    public async Task<OperationResult<GameResponseDto?>> GetGameByIdAsync(Guid id)
    {
        var game = await _gameRepository.GetByIdAsync(id);

        if (game == null)
        {
            _logger.LogWarning("Jogo não encontrado: {GameId}", id);            
            return OperationResult<GameResponseDto?>.Failure("Jogo não encontrado: {GameId}"); 
        }

        return OperationResult<GameResponseDto?>.Success(MapToResponse(game));
    }

    public async Task<OperationResult<PagedResult<GameResponseDto>>> GetAllGamesAsync(PagedRequestDto<GameFilterDto, GameOrderDto> pagedRequestDto)
    {
        // Busca todos os jogos
        var gamesQuery = (await _gameRepository.GetAllAsync()).AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(pagedRequestDto.Filter?.Name))
            gamesQuery = gamesQuery.Where(g => g.Title.Contains(pagedRequestDto.Filter.Name));

        if (!string.IsNullOrWhiteSpace(pagedRequestDto.Filter?.Genre))
            gamesQuery = gamesQuery.Where(g => g.Genre.Contains(pagedRequestDto.Filter.Genre));

        // Ordenação
        string? orderBy = pagedRequestDto.OrderBy?.OrderBy?.ToLower();
        bool ascending = pagedRequestDto.OrderBy?.Ascending ?? true;

        gamesQuery = orderBy switch
        {
            "name" => ascending ? gamesQuery.OrderBy(g => g.Title) : gamesQuery.OrderByDescending(g => g.Title),
            "genre" => ascending ? gamesQuery.OrderBy(g => g.Genre) : gamesQuery.OrderByDescending(g => g.Genre),
            _ => gamesQuery.OrderBy(g => g.Title)
        };

        // Paginação
        var totalItems = gamesQuery.Count();
        var games = gamesQuery
            .Skip((pagedRequestDto.PageNumber - 1) * pagedRequestDto.PageSize)
            .Take(pagedRequestDto.PageSize)
            .ToList();

        var pagedGames = new PagedResult<GameResponseDto>
        {
            Items = games.Select(MapToResponse),
            PageNumber = pagedRequestDto.PageNumber,
            PageSize = pagedRequestDto.PageSize,
            TotalItems = totalItems
        };

        return OperationResult<PagedResult<GameResponseDto>>.Success(pagedGames);
    }

    public async Task<OperationResult> DeleteGameAsync(Guid id)
    {
        var game = await _gameRepository.GetByIdAsync(id);
        if (game == null)
        {
            _logger.LogWarning("Tentativa de exclusão falhou. Jogo não encontrado: {GameId}", id);
            return OperationResult.Failure("Game não encontrado.");
        }

        await _gameRepository.DeleteAsync(game);
        _logger.LogInformation("Jogo excluído com sucesso: {GameId}", id);

        return OperationResult.Success();
    }

    public async Task<bool> GameExistsAsync(Guid id)
    {
        var exists = await _gameRepository.ExistsAsync(id);
        if (!exists)
        {
            _logger.LogDebug("Jogo não encontrado: {GameId}", id);
        }
        return exists;
    }

    public async Task<bool> GameExistsAsync(string ean)
    {
        var exists = await _gameRepository.ExistsAsync(ean);
        if (!exists)
        {
            _logger.LogDebug("Jogo não encontrado: {GameId}", ean);
        }
        return exists;
    }

    #region private methods
    private GameResponseDto MapToResponse(Game game)
    {
        return new GameResponseDto
        {
            Id = game.Id,
            EAN = game.EAN,
            Title = game.Title,
            Genre = game.Genre,
            Description = game.Description
        };
    }
    #endregion
}