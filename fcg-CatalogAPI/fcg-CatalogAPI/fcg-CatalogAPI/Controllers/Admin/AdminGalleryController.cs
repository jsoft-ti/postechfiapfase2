using Api.Controllers.Base;
using Application.Dto.Request;
using Application.Dto.Response;
using Application.Interfaces.Service;
using Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = RoleConstants.Admin)]
[Tags("Admin")]
[Route("api/admin/gallery")]
public class AdminGalleryController : ApiControllerBase
{
    private readonly IGalleryService _galleryService;

    public AdminGalleryController(IGalleryService galleryService)
    {
        _galleryService = galleryService;
    }

    // 🎮 Gallery Management
    [HttpPost]
    public async Task<ActionResult<GalleryGameResponseDto>> AddToGallery([FromBody] GalleryGameCreateRequestDto request)
    {
        var result = await _galleryService.AddToGalleryAsync(request);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return CreatedAtAction(nameof(GetGalleryGame), new { id = result.Data!.Id }, result.Data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GalleryGameResponseDto>> GetGalleryGame(Guid id)
    {
        var result = await _galleryService.GetGalleryGameByIdAsync(id);

        if (!result.Succeeded)
            return NotFound(new { errors = result.Errors });

        return Ok(result.Data);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<GalleryGameResponseDto>> UpdateGalleryGame(Guid id, [FromBody] GalleryGameUpdateRequestDto request)
    {
        var result = await _galleryService.UpdateGalleryGameAsync(id, request);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> RemoveFromGallery(Guid id)
    {
        var result = await _galleryService.RemoveFromGalleryAsync(id);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("{id}/promotion")]
    public async Task<ActionResult> ApplyPromotion(Guid id, [FromBody] GalleryPromotionRequestDto request)
    {
        var result = await _galleryService.ApplyPromotionAsync(
            id,
            request.PromotionType,
            request.PromotionValue,
            request.PromotionStartDate,
            request.PromotionEndDate);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpDelete("{id}/promotion")]
    public async Task<ActionResult> RemovePromotion(Guid id)
    {
        var result = await _galleryService.RemovePromotionAsync(id);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }
}
