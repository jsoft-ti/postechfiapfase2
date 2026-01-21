using Api.Controllers.Base;
using Application.Dto.Filter;
using Application.Dto.Order;
using Application.Dto.Request;
using Application.Dto.Response;
using Application.Dto.Result;
using Application.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;

namespace fcg_CatalogAPI.Controllers;

//[Authorize(Roles = RoleConstants.Player)]
[Route("api/[controller]")]
[ApiController]

public class PurchaseController : ApiControllerBase
{
    private readonly IPurchaseService _purchaseAppService;
    private readonly IGalleryService _gameGalleryService;
    private readonly IMessageService _eventService;
    public PurchaseController(IPurchaseService purchaseAppService, IGalleryService gameGalleryService, IMessageService  eventService)
    {
        _purchaseAppService = purchaseAppService;
        _gameGalleryService = gameGalleryService;
        _eventService = eventService;
    }

    private Guid? GetCurrentUserId()
    {
        // Tenta buscar em várias claims padrão
        var claim = User.FindFirst("sub")?.Value
                       ?? User.FindFirst("nameidentifier")?.Value;

        return Guid.TryParse(claim, out var guid) ? guid : null;
    }

    // 📥 Compra direta de um jogo
    [HttpPost("single")]
    public async Task<IActionResult> RegisterSinglePurchase([FromBody] PurchaseCreateRequestDto dto)
    {
        var playerId = dto.PlayerId;

        if (playerId == Guid.Empty)
            return Unauthorized("Não autorizado.");

        // se o id for fiferente por enquanto não libera, podemos ver para presentear, pelo displayname algo assim...
        if (playerId != dto.PlayerId)        
            return BadRequest("Ainda não é possivel presentear o jogo para outro jogador.");

        var result = await _purchaseAppService.RegisterPurchaseAsync(dto);

        if (result.Succeeded)
        {
            var gameId = dto.GameId;
            OperationResult<decimal> price = await _gameGalleryService.GetGamePriceAsync(gameId);
            
                var opEventResponse = new OrderPlacedEventRequestDto();

                opEventResponse.UserId = playerId;
                opEventResponse.GameId = gameId;
                opEventResponse.Price = price.Data;
                await _eventService.Handle(opEventResponse);
                
            return Ok(result.Data);
        }

        return BadRequest(new { result.Errors });
    }

    // 🛒 Compra de todos os itens do carrinho
    [HttpPost("cart/{playerId}")]
    public async Task<IActionResult> RegisterPurchaseFromCart(Guid playerId)
    {
        //var playerId = GetCurrentUserId();

        if (playerId == Guid.Empty)
            return Unauthorized("Não autorizado.");

        var result = await _purchaseAppService.RegisterPurchaseAsync(playerId);

        if (result.Succeeded)
        {
            //Registrar evento de pagamento
            return Ok();
        }

        return BadRequest(new { result.Errors });
    }

    // 🔍 Listar compras de um jogador
    [HttpGet("player")]
    public async Task<ActionResult<PagedResult<PurchaseResponseDto>>> GetPurchasesByPlayer(PagedRequestDto<PurchaseFilterDto?, PurchaseOrderDto?> dto)
    {
        var playerId = GetCurrentUserId();

        if (playerId == null)
            return Unauthorized("Não autorizado.");

        var purchases = await _purchaseAppService.GetPurchasesByPlayerAsync(playerId.Value, dto);

        return Ok(purchases.Data);
    }
}
