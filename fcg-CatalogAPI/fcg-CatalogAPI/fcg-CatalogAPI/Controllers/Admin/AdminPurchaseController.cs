using Api.Controllers.Base;
using Application.Dto.Filter;
using Application.Dto.Order;
using Application.Dto.Request;
using Application.Dto.Response;
using Application.Dto.Result;
using Application.Interfaces.Service;
using Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Admin;

[ApiController]
//[Authorize(Roles = RoleConstants.Admin)]
[Tags("Admin")]
[Produces("application/json")]
[Route("api/admin/purchases")]
public class AdminPurchaseController : ApiControllerBase
{
    private readonly IPurchaseService _purchaseAppService;
    private readonly ILogger<AdminPurchaseController> _logger;

    public AdminPurchaseController(IPurchaseService purchaseAppService,
                                   ILogger<AdminPurchaseController> logger)
    {
        _purchaseAppService = purchaseAppService;
        _logger = logger;
    }

    /// <summary>
    /// Compra direta de um jogo
    /// </summary>
    [HttpPost("single")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterSinglePurchase([FromBody] PurchaseCreateRequestDto dto)
    {
        _logger.LogInformation("RegisterSinglePurchase called. PlayerId: {PlayerId}, GameId: {GameId}", dto?.PlayerId, dto?.GameId);

        if (dto is null)
            return ErrorResult<PurchaseResponseDto>("Corpo da requisição é obrigatório.");

        if (dto.PlayerId == Guid.Empty || dto.GameId == Guid.Empty)
            return ErrorResult<PurchaseResponseDto>("PlayerId e GameId são obrigatórios.");

        try
        {
            var result = await _purchaseAppService.RegisterPurchaseAsync(dto);
            return ProcessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RegisterSinglePurchase for PlayerId: {PlayerId}", dto.PlayerId);
            return Problem(detail: "Erro interno ao processar a requisição.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Compra de todos os itens do carrinho
    /// </summary>
    [HttpPost("cart/{playerId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterPurchaseFromCart([FromRoute] Guid playerId)
    {
        _logger.LogInformation("RegisterPurchaseFromCart called. PlayerId: {PlayerId}", playerId);

        if (playerId == Guid.Empty)
            return ErrorResult<object>("PlayerId inválido.");

        try
        {
            var result = await _purchaseAppService.RegisterPurchaseAsync(playerId);
            return ProcessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RegisterPurchaseFromCart for PlayerId: {PlayerId}", playerId);
            return Problem(detail: "Erro interno ao processar a requisição.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Listar compras de um jogador
    /// </summary>
    [HttpGet("player/{playerId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPurchasesByPlayer([FromRoute] Guid playerId, [FromQuery] PagedRequestDto<PurchaseFilterDto?, PurchaseOrderDto?> dto)
    {
        _logger.LogInformation("GetPurchasesByPlayer called. PlayerId: {PlayerId} PageNumber: {PageNumber} PageSize: {PageSize}", playerId, dto?.PageNumber, dto?.PageSize);

        if (playerId == Guid.Empty)
            return ErrorResult<PagedResult<PurchaseResponseDto>>("PlayerId inválido.");

        try
        {
            var purchases = await _purchaseAppService.GetPurchasesByPlayerAsync(playerId, dto);
            return ProcessResult(purchases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetPurchasesByPlayer for PlayerId: {PlayerId}", playerId);
            return Problem(detail: "Erro interno ao processar a requisição.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Listar todas as compras (com paginação/filtragem)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllPurchases([FromQuery] PagedRequestDto<PurchaseFilterDto?, PurchaseOrderDto?> dto)
    {
        _logger.LogInformation("GetAllPurchases called. PageNumber: {PageNumber} PageSize: {PageSize}", dto.PageNumber, dto.PageSize);

        try
        {
            var resultPurchases = await _purchaseAppService.GetAllPurchasesAsync(dto);
            return ProcessResult(resultPurchases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllPurchases");
            return Problem(detail: "Erro interno ao processar a requisição.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Estatísticas de compras no período
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPurchaseStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        _logger.LogInformation("GetPurchaseStats called. Start: {StartDate} End: {EndDate}", startDate, endDate);

        try
        {
            var result = await _purchaseAppService.GetPurchaseStatsAsync(startDate, endDate);
            return ProcessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetPurchaseStats.");
            return Problem(detail: "Erro interno ao processar a requisição.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
