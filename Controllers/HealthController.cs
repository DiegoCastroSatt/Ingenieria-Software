using Microsoft.AspNetCore.Mvc;
using Softawer.Data;
using Softawer.Models;

namespace Softawer.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController(DatabaseHealthRepository healthRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HealthResponse>> Get()
    {
        try
        {
            await healthRepository.PingAsync();
            return Ok(new HealthResponse("ok", "API and database connection are available."));
        }
        catch (Exception exception)
        {
            return Problem(
                detail: exception.Message,
                title: "Database connection failed",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}
