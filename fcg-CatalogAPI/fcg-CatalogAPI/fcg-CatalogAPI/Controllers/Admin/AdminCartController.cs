using Api.Controllers.Base;
using Application.Dto.Request;
using Application.Interfaces.Service;
using Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fcg_CatalogAPI.Controllers.Admin;

[ApiController]
[Authorize(Roles = RoleConstants.Admin)]
[Tags("Admin")]
[Route("api/admin/cart")]
public class AdminCartController : ApiControllerBase
{
    private readonly ICartService _cartService;
    public AdminCartController(ICartService cartService)
    {
        _cartService = cartService;
    }
    
    [HttpPost("{playerId}/add/{gameId}")]
    public async Task<IActionResult> AddItem(Guid playerId, Guid galleryId)
    {
        var request = new CartItemRequestDto
        {
            PlayerId = playerId,
            GalleryId = galleryId
        };

        var result = await _cartService.AddItemAsync(request);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }

    [HttpDelete("{playerId}/remove/{gameId}")]
    public async Task<IActionResult> RemoveItem(Guid playerId, Guid galleryId)
    {
        var request = new CartItemRequestDto
        {
            PlayerId = playerId,
            GalleryId = galleryId
        };

        var result = await _cartService.RemoveItemAsync(request);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }

    [HttpGet("{playerId}")]
    public async Task<IActionResult> GetCart(Guid playerId)
    {
        var result = await _cartService.GetCartByPlayerIdAsync(playerId);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Data);
    }

    [HttpDelete("{playerId}/clear")]
    public async Task<IActionResult> ClearCart(Guid playerId)
    {
        var result = await _cartService.ClearCartAsync(playerId);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }
}
