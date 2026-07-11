using Microsoft.AspNetCore.Mvc;
using Softawer.Data;
using Softawer.Models;

namespace Softawer.Controllers;

[ApiController]
[Route("api/metricas")]
public class MetricasController(
    UsuarioRepository usuarioRepository,
    MetricasRepository metricasRepository) : ControllerBase
{
    [HttpGet("usuario/{idUsuario:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<Metrica>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Metrica>>> GetMetricasUsuario(int idUsuario)
    {
        return Ok(await metricasRepository.ListMetricasUsuarioAsync(idUsuario));
    }

    [HttpPost]
    [ProducesResponseType(typeof(Metrica), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Metrica>> CrearMetrica([FromBody] CrearMetricaRequest request)
    {
        if (await usuarioRepository.GetUsuarioAsync(request.IdUsuario) is null)
        {
            return ApiError.NotFound(this, "Usuario no encontrado.");
        }

        if (string.IsNullOrWhiteSpace(request.Ejercicio))
        {
            return ApiError.BadRequest(this, "El ejercicio es obligatorio.");
        }

        if (request.PesoKg <= 0)
        {
            return ApiError.BadRequest(this, "El peso debe ser mayor a 0.");
        }

        return Ok(await metricasRepository.CreateMetricaAsync(request));
    }

    [HttpDelete("{idMetrica:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarMetrica(int idMetrica, [FromQuery] int idUsuario)
    {
        if (idUsuario <= 0)
        {
            return ApiError.BadRequest(this, "El usuario es obligatorio.");
        }

        return await metricasRepository.DeleteMetricaAsync(idMetrica, idUsuario)
            ? NoContent()
            : ApiError.NotFound(this, "Metrica no encontrada.");
    }
}
