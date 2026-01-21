using fcg_UsersAPI.Controllers.Base;
using Application.Dto.Filter;
using Application.Dto.Order;
using Application.Dto.Request;
using Application.Interfaces;
using Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fcg_UsersAPI.Controllers.Admin;

[ApiController]
[Authorize(Roles = RoleConstants.Admin)]
[Tags("Admin")]
[Route("api/admin/user")]
public class AdminUserController : ApiControllerBase
{
    private readonly IUserService _userService;

    public AdminUserController(IUserService userService) {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var result = await _userService.GetByIdAsync(id);

        if (!result.Succeeded)
            return NotFound(new { errors = result.Errors });

        return Ok(result.Data);
    }

    // 👤 Lista todos usuários
    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] PagedRequestDto<UserFilterDto, UserOrderDto> dto)
    {
        var users = await _userService.GetAllAsync(dto);

        return Ok(users.Data);
    }
}
