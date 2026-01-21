using fcg_UsersAPI.Controllers.Base;
using Application.Dto.Filter;
using Application.Dto.Order;
using Application.Dto.Request;
using Application.Interfaces;
using Application.Interfaces.Service;
using Application.Security;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fcg_UsersAPI.Controllers.Admin;

[ApiController]
[Authorize(Roles = RoleConstants.Admin)]
[Tags("Admin")]
[Route("api/admin/[Controller]")]
public class AdminController : ApiControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AdminController(
        IAuthService authService,
        IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    // 🔐 Retorna um admin
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var result = await _userService.GetByIdAsync(id);

        if (!result.Succeeded)
            return NotFound(new { errors = result.Errors });

        return Ok(result.Data);
    }

    // 🔐 Cadastrar novo admin
    [HttpPost]
    public async Task<IActionResult> RegisterAdmin([FromBody] UserCreateRequestDto dto)
    {
        var result = await _authService.RegisterUserAsync(dto, RoleConstants.Admin);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        // ✅ Retorna apenas os dados do usuário criado
        return CreatedAtAction(nameof(GetUserById), new { id = result.Data!.Id }, result.Data);
    }

    // 👤 Listar todos os admins
    [HttpGet]
    public async Task<IActionResult> GetAllAdmins([FromQuery] PagedRequestDto<UserFilterDto, UserOrderDto> dto)
    {
        var result = await _userService.GetUsersByRoleAsync(Roles.Admin, dto);

        return Ok(result.Data);
    }

    // 👤 Atualizar dados de um admin
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAdmin(Guid id, [FromBody] UserUpdateRequestDto dto)
    {
        var result = await _userService.UpdateUserAsync(id, dto);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }

    // 👤 Remover um admin
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAdmin(Guid id)
    {
        var result = await _userService.DeleteUserAsync(id);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }
}