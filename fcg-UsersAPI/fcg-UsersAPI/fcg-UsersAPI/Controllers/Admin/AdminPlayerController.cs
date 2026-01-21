using fcg_UsersAPI.Controllers.Base;
using Application.Dto.Filter;
using Application.Dto.Order;
using Application.Dto.Request;
using Application.Interfaces;
using Application.Security;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fcg_UsersAPI.Controllers.Admin;

[ApiController]
[Authorize(Roles = RoleConstants.Admin)]
[Tags("Admin")]
[Route("api/admin/player")]
public class AdminPlayerController : ApiControllerBase
{
    private readonly IUserService _userService;

    public AdminPlayerController(
        IUserService userService)
    {
        _userService = userService;
    }

    // 👤 Listar todos os players
    [HttpGet]
    public async Task<IActionResult> GetAllPlayers([FromQuery] PagedRequestDto<UserFilterDto, UserOrderDto> dto)
    {
        var result = await _userService.GetUsersByRoleAsync(Roles.Player, dto);

        return Ok(result.Data);
    }

    // 👤 Atualizar dados de um player
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePlayer(Guid id, [FromBody] UserUpdateRequestDto dto)
    {
        var result = await _userService.UpdateUserAsync(id, dto);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }

    // 👤 Remover um player
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayer(Guid id)
    {
        var result = await _userService.DeleteUserAsync(id);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }
}