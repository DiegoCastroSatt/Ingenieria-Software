using Microsoft.AspNetCore.Mvc;
using Softawer.Data;
using Softawer.Models;
using Softawer.Services;

namespace Softawer.Controllers;

[ApiController]
[Route("api/rutinas")]
public class RutinasController(
    RutinaRepository rutinaRepository,
    UsuarioRepository usuarioRepository,
    RutinaCopyService rutinaCopyService) : ControllerBase
{
    [HttpGet("predefinidas")]
    [ProducesResponseType(typeof(IReadOnlyList<RutinaResumenResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RutinaResumenResponse>>> GetPredefinidas()
    {
        return Ok(await rutinaRepository.ListRutinasPredefinidasAsync());
    }

    [HttpGet("usuario/{idUsuario:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<RutinaResumenResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RutinaResumenResponse>>> GetRutinasUsuario(int idUsuario)
    {
        return Ok(await rutinaRepository.ListRutinasUsuarioAsync(idUsuario));
    }

    [HttpGet("{idRutina:int}")]
    [ProducesResponseType(typeof(RutinaDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RutinaDetalleResponse>> GetRutina(int idRutina)
    {
        var rutina = await rutinaRepository.GetRutinaDetalleAsync(idRutina);
        return rutina is null ? ApiError.NotFound(this, "Rutina no encontrada.") : Ok(rutina);
    }

    [HttpPost("personalizadas")]
    [ProducesResponseType(typeof(RutinaDetalleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RutinaDetalleResponse>> CrearPersonalizada([FromBody] CrearRutinaRequest request)
    {
        if (await usuarioRepository.GetUsuarioAsync(request.IdUsuario) is null)
        {
            return ApiError.NotFound(this, "Usuario no encontrado.");
        }

        var idRutina = await rutinaRepository.CreateRutinaAsync(new Rutina
        {
            IdUsuario = request.IdUsuario,
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion.Trim(),
            TipoRutina = "personalizada",
            Objetivo = request.Objetivo.Trim(),
            Dificultad = request.Dificultad.Trim(),
            EsPublica = request.EsPublica
        }, request.Ejercicios);

        var rutina = await rutinaRepository.GetRutinaDetalleAsync(idRutina);
        return CreatedAtAction(nameof(GetRutina), new { idRutina }, rutina);
    }

    [HttpPost("{idRutinaOrigen:int}/copiar")]
    [ProducesResponseType(typeof(RutinaDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RutinaDetalleResponse>> CopiarRutina(int idRutinaOrigen, [FromBody] CopiarRutinaRequest request)
    {
        if (await usuarioRepository.GetUsuarioAsync(request.IdUsuario) is null)
        {
            return ApiError.NotFound(this, "Usuario no encontrado.");
        }

        var origen = await rutinaRepository.GetRutinaDetalleAsync(idRutinaOrigen);
        if (origen is null || origen.TipoRutina != "predefinida")
        {
            return ApiError.BadRequest(this, "La rutina origen debe existir y ser predefinida.");
        }

        var copia = rutinaCopyService.CrearCopiaEditable(origen, request.IdUsuario);
        var nuevaRutina = await rutinaRepository.CopyRutinaAsync(copia, request.ActivarRutina);
        return Ok(nuevaRutina);
    }

    [HttpPut("{idRutina:int}")]
    [ProducesResponseType(typeof(RutinaDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RutinaDetalleResponse>> EditarRutina(int idRutina, [FromBody] ActualizarRutinaRequest request)
    {
        if (!await rutinaRepository.UsuarioPuedeEditarRutinaAsync(idRutina, request.IdUsuario))
        {
            return ApiError.Forbidden(this, "Solo puedes editar rutinas propias personalizadas o copiadas.");
        }

        var rutina = await rutinaRepository.UpdateRutinaAsync(idRutina, request);
        return rutina is null ? ApiError.NotFound(this, "Rutina no encontrada.") : Ok(rutina);
    }

    [HttpDelete("{idRutina:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EliminarRutina(int idRutina, [FromQuery] int idUsuario)
    {
        if (idUsuario <= 0)
        {
            return ApiError.BadRequest(this, "El usuario es obligatorio.");
        }

        var deleted = await rutinaRepository.DeleteRutinaAsync(idRutina, idUsuario);
        return deleted ? NoContent() : ApiError.Forbidden(this, "Solo puedes eliminar rutinas propias personalizadas o copiadas.");
    }
}
