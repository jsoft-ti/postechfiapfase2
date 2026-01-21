using Microsoft.AspNetCore.Mvc;

namespace fcg_CatalogAPI.Controllers;

public class HealthController : ControllerBase
{
    [HttpGet()]
    public IActionResult Get() => Ok("Healthy");
}