using matdev.Application.Models;
using matdev.Infrastructure.Data;
using matdev.Infrastructure.Data.Seeds;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace matdev.API.Controllers;

/// <summary>Demo / mock data helpers (Development or DemoSeed:Enabled).</summary>
[ApiController]
[Route("api/dev")]
public class DevController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public DevController(ApplicationDbContext db, IWebHostEnvironment env, IConfiguration config)
    {
        _db = db;
        _env = env;
        _config = config;
    }

    private bool IsDemoEndpointsEnabled =>
        _env.IsDevelopment() || _config.GetValue("DemoSeed:Enabled", false);

    [HttpPost("seed-demo")]
    [ProducesResponseType(typeof(ResponseModel<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object>>> SeedDemo()
    {
        if (!IsDemoEndpointsEnabled)
            return NotFound();

        await DemoSeedData.EnsureLookupsAsync(_db);
        await DemoSeedData.SeedDemoProjectsAsync(_db);

        var projectCount = await _db.Projects.CountAsync();
        var hasDemo = await _db.Projects.AnyAsync(p => p.Name == DemoSeedData.DemoMainProjectName);
        return Ok(new ResponseModel<object>
        {
            Data = new
            {
                projectCount,
                hasDemoProject = hasDemo,
                message = hasDemo
                    ? "Demo projects available."
                    : "Demo projects seeded.",
            },
            Message = "Demo seed completed.",
        });
    }

    [HttpPost("reset-demo")]
    [ProducesResponseType(typeof(ResponseModel<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object>>> ResetDemo()
    {
        if (!IsDemoEndpointsEnabled)
            return NotFound();

        await DemoSeedData.ResetAndSeedAsync(_db);

        var projectCount = await _db.Projects.CountAsync();
        return Ok(new ResponseModel<object>
        {
            Data = new { projectCount },
            Message = "Database reset and demo data loaded.",
        });
    }
}
