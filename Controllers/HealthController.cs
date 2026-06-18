using Microsoft.AspNetCore.Mvc;
using Softawer.Data;
using Softawer.Models;

namespace Softawer.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController(DatabaseHealthRepository healthRepository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<HealthResponse>> Get()
    {
        try
        {
            await healthRepository.PingAsync();
            return Ok(new HealthResponse("ok", "API and database connection are available."));
        }
        catch
        {
            return ApiError.ServiceUnavailable(this, "No se pudo verificar la conexion con los servicios requeridos.");
        }
    }
}
