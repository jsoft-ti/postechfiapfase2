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
[Route("api/admin/game")]
public class AdminGameController : ApiControllerBase
{
    private readonly IGameService _gameService;

    public AdminGameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    // 🕹️ Game Management
    [HttpPost]
    public async Task<IActionResult> CreateGame([FromBody] GameCreateRequestDto dto)
    {
        var result = await _gameService.CreateGameAsync(dto);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return CreatedAtAction(nameof(GetGame), new { id = result.Data!.Id }, result.Data);
    }

    // 🕹️ Game Management
    [HttpGet("{id}")]
    public async Task<IActionResult> GetGame(Guid id)
    {
        var result = await _gameService.GetGameByIdAsync(id);

        if (!result.Succeeded)
            return NotFound(new { errors = result.Errors });

        return Ok(result.Data);
    }

    // 🕹️ Game Management
    [HttpGet]
    public async Task<ActionResult<PagedResult<GameResponseDto>>> GetGames([FromQuery] PagedRequestDto<GameFilterDto, GameOrderDto> dto)
    {
        var result = await _gameService.GetAllGamesAsync(dto);

        if (!result.Succeeded)
            return NotFound(new { errors = result.Errors });

        return Ok(result.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGame(Guid id, [FromBody] GameUpdateRequestDto dto)
    {
        var result = await _gameService.UpdateGameAsync(id, dto);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    // 🕹️ Game Management
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGame(Guid id)
    {
        var result = await _gameService.DeleteGameAsync(id);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }
}