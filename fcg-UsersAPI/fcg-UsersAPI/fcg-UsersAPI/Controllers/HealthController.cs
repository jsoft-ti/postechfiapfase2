using Microsoft.AspNetCore.Mvc;

namespace fcg_UsersAPI.Controllers;

public class HealthController : ControllerBase
{
    [HttpGet()]
    public IActionResult Get() => Ok("Healthy");
}