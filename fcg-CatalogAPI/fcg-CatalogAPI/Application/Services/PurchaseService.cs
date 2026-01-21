using Application.Dto.Filter;
using Application.Dto.Order;
using Application.Dto.Request;
using Application.Dto.Response;
using Application.Dto.Result;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using Domain.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class PurchaseService : IPurchaseService
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IGalleryRepository _galleryRepository;
    private readonly ILibraryRepository _libraryRepository;
    private readonly ICartRepository _cartRepository;
    private readonly ILogger<PurchaseService> _logger;

    public PurchaseService(
        IPlayerRepository playerRepository,
        IGalleryRepository galleryRepository,
        ILibraryRepository libraryRepository,
        ICartRepository cartRepository,
        ILogger<PurchaseService> logger)
    {
        _playerRepository = playerRepository;
        _galleryRepository = galleryRepository;
        _libraryRepository = libraryRepository;
        _cartRepository = cartRepository;
        _logger = logger;
    }

    public async Task<OperationResult<PurchaseResponseDto>> RegisterPurchaseAsync(PurchaseCreateRequestDto dto)
    {
        var player = await _playerRepository.GetByIdAsync(dto.PlayerId);
        if (player == null)
            return OperationResult<PurchaseResponseDto>.Failure("Jogador não encontrado.");

        var galleryGame = await _galleryRepository.GetGalleryGameByIdAsync(dto.GameId);
        if (galleryGame == null)
            return OperationResult<PurchaseResponseDto>.Failure("Jogo não encontrado na galeria.");

        if (!await _galleryRepository.IsAvailableForPurchaseAsync(dto.GameId))
            return OperationResult<PurchaseResponseDto>.Failure("Jogo não está disponível para compra.");

        if (await _libraryRepository.HasGameInLibraryAsync(dto.PlayerId, galleryGame.Game.Title))
            return OperationResult<PurchaseResponseDto>.Failure("Jogador já possui este jogo.");

        
        var libraryGame = new LibraryGame(galleryGame.Id, player.Id, galleryGame.FinalPrice);
        //await _libraryRepository.AddToLibraryAsync(libraryGame); //Não Salvar na library se não tiver pagamento aprovado

        _logger.LogInformation("Compra registrada: Player {PlayerId}, Game {GameId}, Preço {Price}",
            player.Id, galleryGame.Id, galleryGame.FinalPrice);

        var response = new PurchaseResponseDto
        {
            PurchaseId = libraryGame.Id,
            PlayerName = player.DisplayName,
            GameName = galleryGame.Game.Title,
            Price = galleryGame.FinalPrice,
            PurchaseDate = libraryGame.PurchaseDate
        };

        return OperationResult<PurchaseResponseDto>.Success(response);
    }

    public async Task<OperationResult> RegisterPurchaseAsync(Guid playerId)
    {
        var cart = await _cartRepository.GetByPlayerIdAsync(playerId);
        if (cart == null || !cart.Items.Any())
            return OperationResult.Failure("Carrinho vazio.");

        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
            return OperationResult.Failure("Jogador não encontrado.");

        foreach (var item in cart.Items)
        {
            var galleryGame = await _galleryRepository.GetGalleryGameByIdAsync(item.GalleryId);
            if (galleryGame == null || !await _galleryRepository.IsAvailableForPurchaseAsync(item.GalleryId))
                continue;

            if (await _libraryRepository.HasGameInLibraryAsync(playerId, galleryGame.Game.Title))
                continue;

            var libraryGame = new LibraryGame(galleryGame.Id, player.Id, galleryGame.FinalPrice);
            await _libraryRepository.AddToLibraryAsync(libraryGame);

            _logger.LogInformation("Compra registrada via carrinho: Player {PlayerId}, Game {GameId}, Preço {Price}",
                player.Id, galleryGame.Id, galleryGame.FinalPrice);
        }

        cart.Clear();
        await _cartRepository.UpdateAsync(cart);

        return OperationResult.Success();
    }

    public async Task<OperationResult<PagedResult<PurchaseResponseDto>>> GetPurchasesByPlayerAsync(Guid playerId, PagedRequestDto<PurchaseFilterDto?, PurchaseOrderDto?> dto)
    {
        try
        {
            var libraryGame = await _libraryRepository.GetPlayerLibraryAsync(playerId);
            var purchase = libraryGame.Select(g => new PurchaseResponseDto
            {
                PurchaseId = g.Id,
                PlayerName = g.Player?.DisplayName ?? string.Empty,
                GameName = g.Gallery.Game.Title,
                Price = g.PurchasePrice,
                PurchaseDate = g.PurchaseDate
            }).OrderByDescending(p => p.PurchaseDate);

            var totalItems = purchase.Count();

            var pagedPurchases = new PagedResult<PurchaseResponseDto>
            {
                Items = purchase,
                PageNumber = dto.PageNumber,
                PageSize = dto.PageSize,
                TotalItems = totalItems
            };

            return OperationResult<PagedResult<PurchaseResponseDto>>.Success(pagedPurchases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todas as compras do player");
            return OperationResult<PagedResult<PurchaseResponseDto>>.Failure("Erro ao buscar todas as compras do player");
        }
    }

    public async Task<OperationResult<IEnumerable<PurchaseResponseDto>>> GetAllPurchasesAsync()
    {
        try
        {
            var libraryGame = await _libraryRepository.GetAllPurchasesAsync();
            var purchase = libraryGame.Select(g => new PurchaseResponseDto
            {
                PurchaseId = g.Id,
                PlayerName = g.Player?.DisplayName ?? string.Empty,
                GameName = g.Gallery.Game.Title,
                Price = g.PurchasePrice,
                PurchaseDate = g.PurchaseDate
            });

            return OperationResult<IEnumerable<PurchaseResponseDto>>.Success(purchase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todas as compras");
            return OperationResult<IEnumerable<PurchaseResponseDto>>.Failure("Erro ao buscar todas as compras");
        }
    }

    public async Task<OperationResult<PagedResult<PurchaseResponseDto>>> GetAllPurchasesAsync(PagedRequestDto<PurchaseFilterDto?, PurchaseOrderDto?> dto)
    {
        try
        {
            var pagedPurchases = await GetAllPurchasesPagedAsync(dto, null);

            return OperationResult<PagedResult<PurchaseResponseDto>>.Success(pagedPurchases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar compras paginadas");
            return OperationResult<PagedResult<PurchaseResponseDto>>.Failure("Erro ao buscar compras paginadas");
        }
    }

    public async Task<OperationResult<PurchaseStatsDto>> GetPurchaseStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var totalCount = await _libraryRepository.GetTotalPurchaseCountAsync();
            var totalRevenue = await _libraryRepository.GetTotalRevenueAsync();
            decimal periodRevenue = 0;

            if (startDate.HasValue && endDate.HasValue)
            {
                periodRevenue = await _libraryRepository.GetRevenueInPeriodAsync(startDate.Value, endDate.Value);
            }

            return OperationResult<PurchaseStatsDto>.Success(new PurchaseStatsDto
            {
                TotalPurchases = totalCount,
                TotalRevenue = totalRevenue,
                PeriodRevenue = periodRevenue,
                Period = startDate.HasValue && endDate.HasValue
                    ? $"{startDate.Value:d} - {endDate.Value:d}"
                    : "All time"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar estatísticas de compras");
            return OperationResult<PurchaseStatsDto>.Failure("Erro ao buscar estatísticas de compras");
        }
    }

    #region private methods
    private async Task<PagedResult<PurchaseResponseDto>> GetAllPurchasesPagedAsync(PagedRequestDto<PurchaseFilterDto?, PurchaseOrderDto?> dto, Guid? playerId = null)
    {
        var query = _libraryRepository.GetAllPurchasesQueryable();

        if (playerId != null)
        {
            query = query.Where(p => p.PlayerId == playerId);
        }

        // 🔍 Filtros
        if (dto.Filter != null)
        {
            if (!string.IsNullOrWhiteSpace(dto.Filter.PlayerName))
                query = query.Where(p => p.Player.DisplayName.Contains(dto.Filter.PlayerName));

            if (!string.IsNullOrWhiteSpace(dto.Filter.GameName))
                query = query.Where(p => p.Gallery.Game.Title.Contains(dto.Filter.GameName));

            if (dto.Filter.MinPrice.HasValue)
                query = query.Where(p => p.PurchasePrice >= dto.Filter.MinPrice.Value);

            if (dto.Filter.MaxPrice.HasValue)
                query = query.Where(p => p.PurchasePrice <= dto.Filter.MaxPrice.Value);

            if (dto.Filter.StartDate.HasValue)
                query = query.Where(p => p.PurchaseDate >= dto.Filter.StartDate.Value);

            if (dto.Filter.EndDate.HasValue)
                query = query.Where(p => p.PurchaseDate <= dto.Filter.EndDate.Value);
        }

        // 🔃 Ordenação
        if (dto.OrderBy != null)
        {
            query = dto.OrderBy.SortBy.ToLower() switch
            {
                "playername" => dto.OrderBy.Ascending ? query.OrderBy(p => p.Player.DisplayName) : query.OrderByDescending(p => p.Player.DisplayName),
                "gamename" => dto.OrderBy.Ascending ? query.OrderBy(p => p.Gallery.Game.Title) : query.OrderByDescending(p => p.Gallery.Game.Title),
                "price" => dto.OrderBy.Ascending ? query.OrderBy(p => p.PurchasePrice) : query.OrderByDescending(p => p.PurchasePrice),
                "date" => dto.OrderBy.Ascending ? query.OrderBy(p => p.PurchaseDate) : query.OrderByDescending(p => p.PurchaseDate),
                _ => query.OrderByDescending(p => p.PurchaseDate)
            };
        }

        // 📄 Paginação
        var totalItems = await query.CountAsync();
        var items = await query
            .Skip((dto.PageNumber - 1) * dto.PageSize)
            .Take(dto.PageSize)
            .Select(g => new PurchaseResponseDto
            {
                PurchaseId = g.Id,
                PlayerName = g.Player.DisplayName,
                GameName = g.Gallery.Game.Title,
                Price = g.PurchasePrice,
                PurchaseDate = g.PurchaseDate
            })
            .ToListAsync();

        var pagedPurchases = new PagedResult<PurchaseResponseDto>
        {
            Items = items,
            PageNumber = dto.PageNumber,
            PageSize = dto.PageSize,
            TotalItems = totalItems
        };

        return pagedPurchases;
    }
    #endregion
}