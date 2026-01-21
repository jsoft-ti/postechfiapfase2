using System.Security.Claims;
using Api.Controllers.Base;
using Application.Dto.Request;
using Application.Dto.Response;
using Application.Interfaces.Service;
using Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fcg_CatalogAPI.Controllers;

[ApiController]
//[Authorize(Roles = RoleConstants.Player)]
[Route("api/cart")]
public class CartController : ApiControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpPost("add")]

    [HttpPost("add/{userId}/{gameId}")]
    public async Task<IActionResult> AddItem(Guid userId, Guid gameId)
    {
        //var userId = GetCurrentUserId();

        if (userId == Guid.Empty)
            return UnauthorizedResult<object>("Código do usuário não informado.");

        var request = new CartItemRequestDto
        {
            PlayerId = userId,
            GalleryId = gameId
        };

        var result = await _cartService.AddItemAsync(request);

        return ProcessResult(result);
    }

    [HttpDelete("remove/{gameId}")]
    public async Task<IActionResult> RemoveItem(Guid gameId)
    {
        var playerId = GetCurrentUserId();

        if (playerId == null)
            return UnauthorizedResult<object>("Não autorizado.");

        var request = new CartItemRequestDto
        {
            PlayerId = playerId.Value,
            GalleryId = gameId
        };

        var result = await _cartService.RemoveItemAsync(request);

        return ProcessResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var playerId = GetCurrentUserId();

        if (playerId == null)
            return UnauthorizedResult<CartResponseDto>("Não autorizado.");

        var result = await _cartService.GetCartByPlayerIdAsync(playerId.Value);

        return ProcessResult(result);
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart()
    {
        var playerId = GetCurrentUserId();

        if (playerId == null)
            return UnauthorizedResult<object>("Não autorizado.");

        var result = await _cartService.ClearCartAsync(playerId.Value);

        return ProcessResult(result);
    }

    private Guid? GetCurrentUserId()
    {
        // Tenta buscar em várias claims padrão
        var claimOld = User.FindFirst("sub")?.Value
                       ?? User.FindFirst("nameidentifier")?.Value;

        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(claim, out var guid) ? guid : null;
    }
}
