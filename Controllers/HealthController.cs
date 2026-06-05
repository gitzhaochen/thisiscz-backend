using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ThisisczApi.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext context;

    public HealthController(ApplicationDbContext context)
    {
        this.context = context;
    }

    [HttpGet("live")]
    public ActionResult<object> GetServiceLiveness()
    {
        return Ok(
            new
            {
                status = "ok",
                service = "alive",
                checkedAtUtc = DateTime.UtcNow,
            }
        );
    }

    [HttpGet("database")]
    public async Task<ActionResult<object>> GetDatabaseConnectivity()
    {
        var dbConnected = await context.Database.CanConnectAsync();

        if (!dbConnected)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    status = "unhealthy",
                    database = "disconnected",
                    checkedAtUtc = DateTime.UtcNow,
                }
            );
        }

        return Ok(
            new
            {
                status = "ok",
                database = "connected",
                checkedAtUtc = DateTime.UtcNow,
            }
        );
    }
}
