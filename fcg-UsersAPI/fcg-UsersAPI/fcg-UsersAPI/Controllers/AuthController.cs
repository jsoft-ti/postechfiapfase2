using fcg_UsersAPI.Controllers.Base;
using Application.Dto.Request;
using Application.Dto.Response;

using Application.Interfaces.Service;
using Application.Security;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
namespace fcg_UsersAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;
    private readonly IMessageService _eventService;

    public AuthController(IAuthService authService, IMessageService  eventService)
    {
        _authService = authService;
        _eventService = eventService;
    }

    [HttpPost("register-player")]
    public async Task<IActionResult> Register([FromBody] UserCreateRequestDto dto)
    {
        var result = await _authService.RegisterUserAsync(dto, RoleConstants.Player);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });
        var userRegisterResultDto = new UserRegisterResultDto();
        userRegisterResultDto.Guid = result.Data.Id;
        userRegisterResultDto.Nome = result.Data.FirstName;
        userRegisterResultDto.Email = result.Data.Email;
        
        await _eventService.Handle(userRegisterResultDto);
        var loginResult = await _authService.LoginAsync(new UserLoginRequestDto
        {
            Email = dto.Email,
            Password = dto.Password
        });

        if (!loginResult.Succeeded)
            return BadRequest(new { errors = loginResult.Errors });

        
        return Ok(loginResult.Data);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginAsync(dto);

        if (!result.Succeeded)
            return Unauthorized(new { message = result.Errors });
        
        return Ok(result.Data);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RefreshTokenAsync(dto);

        if (!result.Succeeded)
            return Unauthorized(new { message = result.Errors });

        return Ok(result.Data);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var result = await _authService.GetCurrentUserAsync(User);

        if (!result.Succeeded)
            return NotFound(new { message = "Usuário não encontrado" });

        return Ok(result.Data);
    }
}