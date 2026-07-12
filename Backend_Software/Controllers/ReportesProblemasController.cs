using Microsoft.AspNetCore.Mvc;
using Softawer.Data;
using Softawer.Models;

namespace Softawer.Controllers;

[ApiController]
[Route("api/reportes-problemas")]
public class ReportesProblemasController(
    UsuarioRepository usuarioRepository,
    CatalogoRepository catalogoRepository,
    ReporteProblemaRepository reporteProblemaRepository) : ControllerBase
{
    private const int DescripcionMaxima = 500;

    [HttpGet("{idReporte:int}")]
    [ProducesResponseType(typeof(ReporteProblema), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReporteProblema>> GetReporte(int idReporte)
    {
        var reporte = await reporteProblemaRepository.GetReporteAsync(idReporte);
        return reporte is null ? ApiError.NotFound(this, "Reporte no encontrado.") : Ok(reporte);
    }

    [HttpGet("usuario/{idUsuario:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<ReporteProblema>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ReporteProblema>>> GetReportesUsuario(int idUsuario)
    {
        if (await usuarioRepository.GetUsuarioAsync(idUsuario) is null)
        {
            return ApiError.NotFound(this, "Usuario no encontrado.");
        }

        return Ok(await reporteProblemaRepository.ListReportesUsuarioAsync(idUsuario));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReporteProblema), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReporteProblema>> CrearReporte([FromBody] CrearReporteProblemaRequest request)
    {
        if (await usuarioRepository.GetUsuarioAsync(request.IdUsuario) is null)
        {
            return ApiError.NotFound(this, "Usuario no encontrado.");
        }

        if (request.IdMaquina is int idMaquina && await catalogoRepository.GetMaquinaAsync(idMaquina) is null)
        {
            return ApiError.NotFound(this, "Maquina no encontrada.");
        }

        var descripcion = request.Descripcion.Trim();
        if (descripcion.Length == 0)
        {
            return ApiError.BadRequest(this, "La descripcion del problema es obligatoria.");
        }

        if (descripcion.Length > DescripcionMaxima)
        {
            return ApiError.BadRequest(this, $"La descripcion no puede superar {DescripcionMaxima} caracteres.");
        }

        request.Descripcion = descripcion;
        var reporte = await reporteProblemaRepository.CreateReporteAsync(request);
        return CreatedAtAction(nameof(GetReporte), new { idReporte = reporte.IdReporte }, reporte);
    }
}
