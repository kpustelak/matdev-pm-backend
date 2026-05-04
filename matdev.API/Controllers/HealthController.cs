using Microsoft.AspNetCore.Mvc;

namespace matdev.API.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() =>
        Ok(new { ok = true, utc = DateTime.UtcNow });
}
