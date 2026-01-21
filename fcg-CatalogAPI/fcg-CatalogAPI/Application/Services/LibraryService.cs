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

public class LibraryService : ILibraryService
{
    private readonly ILibraryRepository _libraryRepository;
    private readonly IGalleryRepository _galleryRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<LibraryService> _logger;

    public LibraryService(
        ILibraryRepository libraryRepository,
        IGalleryRepository galleryRepository,
        IPlayerRepository playerRepository,
        ILogger<LibraryService> logger)
    {
        _libraryRepository = libraryRepository;
        _galleryRepository = galleryRepository;
        _playerRepository = playerRepository;
        _logger = logger;
    }

    public async Task<OperationResult<LibraryGameResponseDto>> AddToLibraryAsync(Guid playerId, Guid galleryGameId, decimal purchasePrice)
    {
        try
        {
            var galleryGame = await _galleryRepository.GetGalleryGameByIdAsync(galleryGameId);
            if (galleryGame == null)
                return OperationResult<LibraryGameResponseDto>.Failure("Jogo não encontrado na galeria.");

            if (await _libraryRepository.HasGameInLibraryAsync(playerId, galleryGame.Game.Title))
                return OperationResult<LibraryGameResponseDto>.Failure("Jogador já possui este jogo.");

            var player = await _playerRepository.GetByIdAsync(playerId);
            if (player == null)
                return OperationResult<LibraryGameResponseDto>.Failure("Jogador não encontrado.");

            var libraryGame = new LibraryGame(galleryGame.Id, player.Id, purchasePrice);
            var libraryResult = await _libraryRepository.AddToLibraryAsync(libraryGame);

            if (libraryResult.Item1)
            {
                _logger.LogInformation("Jogo Não fooi adicionado à biblioteca: {GameId} do jogador: {PlayerId}", galleryGameId, playerId);
            }
            else
            {
                _logger.LogInformation("Jogo adicionado à biblioteca: {GameId} para o jogador: {PlayerId}", galleryGameId, playerId);
            }

            return OperationResult<LibraryGameResponseDto>.Success(MapToResponse(libraryGame));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar jogo à biblioteca: {GameId} para o jogador: {PlayerId}", galleryGameId, playerId);
            return OperationResult<LibraryGameResponseDto>.Failure("Erro ao adicionar jogo à biblioteca.");
        }
    }

    public async Task<OperationResult<LibraryGameResponseDto>> GetLibraryGameAsync(Guid playerId, Guid gameId)
    {
        try
        {
            var libraryGame = await _libraryRepository.GetLibraryGameAsync(playerId, gameId);
            if (libraryGame == null)
            {
                _logger.LogWarning("Jogo não encontrado na biblioteca: {GameId} para o jogador: {PlayerId}", gameId, playerId);
                return OperationResult<LibraryGameResponseDto>.Failure("Jogo não encontrado na biblioteca.");
            }

            return OperationResult<LibraryGameResponseDto>.Success(MapToResponse(libraryGame));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar jogo na biblioteca: {GameId} para o jogador: {PlayerId}", gameId, playerId);
            return OperationResult<LibraryGameResponseDto>.Failure("Erro ao buscar jogo na biblioteca.");
        }
    }

    public async Task<OperationResult<PagedResult<LibraryGameResponseDto>>> GetPlayerLibraryAsync(Guid playerId, PagedRequestDto<GameFilterDto, GameOrderDto> pagedRequestDto)
    {
        try
        {
            var libraryGame = (await _libraryRepository.GetPlayerLibraryAsync(playerId)).AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(pagedRequestDto.Filter?.Name))
                libraryGame = libraryGame.Where(g => g.Gallery.Game.Title.Contains(pagedRequestDto.Filter.Name));

            if (!string.IsNullOrWhiteSpace(pagedRequestDto.Filter?.Genre))
                libraryGame = libraryGame.Where(g => g.Gallery.Game.Genre.Contains(pagedRequestDto.Filter.Genre));

            // Apply ordering
            string? orderBy = pagedRequestDto.OrderBy?.OrderBy?.ToLower();
            bool ascending = pagedRequestDto.OrderBy?.Ascending ?? true;

            libraryGame = orderBy switch
            {
                "name" => ascending ? libraryGame.OrderBy(g => g.Gallery.Game.Title) : libraryGame.OrderByDescending(g => g.Gallery.Game.Title),
                "genre" => ascending ? libraryGame.OrderBy(g => g.Gallery.Game.Genre) : libraryGame.OrderByDescending(g => g.Gallery.Game.Genre),
                "purchasedate" => ascending ? libraryGame.OrderBy(g => g.PurchaseDate) : libraryGame.OrderByDescending(g => g.PurchaseDate),
                _ => libraryGame.OrderByDescending(g => g.PurchaseDate)
            };

            // Apply pagination
            var totalItems = libraryGame.Count();
            var pagedGames = libraryGame
                .Skip((pagedRequestDto.PageNumber - 1) * pagedRequestDto.PageSize)
                .Take(pagedRequestDto.PageSize)
                .ToList();

            var result = new PagedResult<LibraryGameResponseDto>
            {
                Items = pagedGames.Select(MapToResponse),
                PageNumber = pagedRequestDto.PageNumber,
                PageSize = pagedRequestDto.PageSize,
                TotalItems = totalItems
            };

            return OperationResult<PagedResult<LibraryGameResponseDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar biblioteca do jogador: {PlayerId}", playerId);
            return OperationResult<PagedResult<LibraryGameResponseDto>>.Failure("Erro ao buscar biblioteca do jogador.");
        }
    }

    public async Task<OperationResult<bool>> OwnsGameAsync(Guid playerId, Guid gameId)
    {
        try
        {
            var ownsLibraryGame = await _libraryRepository.OwnsLibraryGameAsync(playerId, gameId);
            return OperationResult<bool>.Success(ownsLibraryGame);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar propriedade do jogo: {GameId} para o jogador: {PlayerId}", gameId, playerId);
            return OperationResult<bool>.Failure("Erro ao verificar propriedade do jogo.");
        }
    }

    public async Task<OperationResult<bool>> CanPurchaseGameAsync(Guid playerId, Guid gameId)
    {
        try
        {
            var galleryGame = await _galleryRepository.GetGalleryGameByIdAsync(gameId);
            if (galleryGame == null)
                return OperationResult<bool>.Success(false);

            var ownsLibrary = await _libraryRepository.OwnsLibraryGameAsync(playerId, gameId);
            var isAvailable = await _galleryRepository.IsAvailableForPurchaseAsync(gameId);

            return OperationResult<bool>.Success(!ownsLibrary && isAvailable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar disponibilidade para compra: {GameId} para o jogador: {PlayerId}", gameId, playerId);
            return OperationResult<bool>.Failure("Erro ao verificar disponibilidade para compra.");
        }
    }

    public async Task<OperationResult<int>> GetLibraryGameCountAsync(Guid playerId)
    {
        try
        {
            var count = await _libraryRepository.GetLibraryGameCountAsync(playerId);
            return OperationResult<int>.Success(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao contar jogos na biblioteca do jogador: {PlayerId}", playerId);
            return OperationResult<int>.Failure("Erro ao contar jogos na biblioteca.");
        }
    }

    public async Task<OperationResult<decimal>> GetTotalSpentAsync(Guid playerId)
    {
        try
        {
            var total = await _libraryRepository.GetTotalSpentAsync(playerId);
            return OperationResult<decimal>.Success(total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao calcular total gasto pelo jogador: {PlayerId}", playerId);
            return OperationResult<decimal>.Failure("Erro ao calcular total gasto.");
        }
    }

    public async Task<OperationResult<IEnumerable<LibraryGameResponseDto>>> GetRecentPurchasesAsync(Guid playerId, int count = 5)
    {
        try
        {
            var libraryGame = await _libraryRepository.GetRecentPurchasesAsync(playerId, count);
            var response = libraryGame.Select(MapToResponse);
            return OperationResult<IEnumerable<LibraryGameResponseDto>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar compras recentes do jogador: {PlayerId}", playerId);
            return OperationResult<IEnumerable<LibraryGameResponseDto>>.Failure("Erro ao buscar compras recentes.");
        }
    }

    private LibraryGameResponseDto MapToResponse(LibraryGame libraryGame)
    {
        return new LibraryGameResponseDto
        {
            Id = libraryGame.Id,
            EAN = libraryGame.Gallery.Game.EAN,
            Title = libraryGame.Gallery.Game.Title,
            Genre = libraryGame.Gallery.Game.Genre,
            Description = libraryGame.Gallery.Game.Description,
            PlayerId = libraryGame.PlayerId,
            PlayerDisplayName = libraryGame.Player?.DisplayName ?? string.Empty,
            PurchaseDate = libraryGame.PurchaseDate,
            PurchasePrice = libraryGame.PurchasePrice
        };
    }
}