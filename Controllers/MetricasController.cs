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
    public async Task<ActionResult<IReadOnlyList<Metrica>>> GetMetricasUsuario(int idUsuario)
    {
        return Ok(await metricasRepository.ListMetricasUsuarioAsync(idUsuario));
    }

    [HttpPost]
    public async Task<ActionResult<Metrica>> CrearMetrica([FromBody] CrearMetricaRequest request)
    {
        if (await usuarioRepository.GetUsuarioAsync(request.IdUsuario) is null)
        {
            return NotFound("Usuario no encontrado.");
        }

        if (string.IsNullOrWhiteSpace(request.Ejercicio))
        {
            return BadRequest("El ejercicio es obligatorio.");
        }

        if (request.PesoKg <= 0)
        {
            return BadRequest("El peso debe ser mayor a 0.");
        }

        return Ok(await metricasRepository.CreateMetricaAsync(request));
    }

    [HttpDelete("{idMetrica:int}")]
    public async Task<IActionResult> EliminarMetrica(int idMetrica, [FromQuery] int idUsuario)
    {
        if (idUsuario <= 0)
        {
            return BadRequest("El usuario es obligatorio.");
        }

        return await metricasRepository.DeleteMetricaAsync(idMetrica, idUsuario)
            ? NoContent()
            : NotFound("Metrica no encontrada.");
    }
}
