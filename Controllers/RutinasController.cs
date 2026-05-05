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
    public async Task<ActionResult<IReadOnlyList<RutinaResumenResponse>>> GetPredefinidas()
    {
        return Ok(await rutinaRepository.ListRutinasPredefinidasAsync());
    }

    [HttpGet("usuario/{idUsuario:int}")]
    public async Task<ActionResult<IReadOnlyList<RutinaResumenResponse>>> GetRutinasUsuario(int idUsuario)
    {
        return Ok(await rutinaRepository.ListRutinasUsuarioAsync(idUsuario));
    }

    [HttpGet("{idRutina:int}")]
    public async Task<ActionResult<RutinaDetalleResponse>> GetRutina(int idRutina)
    {
        var rutina = await rutinaRepository.GetRutinaDetalleAsync(idRutina);
        return rutina is null ? NotFound("Rutina no encontrada.") : Ok(rutina);
    }

    [HttpPost("personalizadas")]
    public async Task<ActionResult<RutinaDetalleResponse>> CrearPersonalizada([FromBody] CrearRutinaRequest request)
    {
        if (await usuarioRepository.GetUsuarioAsync(request.IdUsuario) is null)
        {
            return NotFound("Usuario no encontrado.");
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
    public async Task<ActionResult<RutinaDetalleResponse>> CopiarRutina(int idRutinaOrigen, [FromBody] CopiarRutinaRequest request)
    {
        if (await usuarioRepository.GetUsuarioAsync(request.IdUsuario) is null)
        {
            return NotFound("Usuario no encontrado.");
        }

        var origen = await rutinaRepository.GetRutinaDetalleAsync(idRutinaOrigen);
        if (origen is null || origen.TipoRutina != "predefinida")
        {
            return BadRequest("La rutina origen debe existir y ser predefinida.");
        }

        var copia = rutinaCopyService.CrearCopiaEditable(origen, request.IdUsuario);
        var nuevaRutina = await rutinaRepository.CopyRutinaAsync(copia, request.ActivarRutina);
        return Ok(nuevaRutina);
    }

    [HttpPut("{idRutina:int}")]
    public async Task<ActionResult<RutinaDetalleResponse>> EditarRutina(int idRutina, [FromBody] ActualizarRutinaRequest request)
    {
        if (!await rutinaRepository.UsuarioPuedeEditarRutinaAsync(idRutina, request.IdUsuario))
        {
            return Forbid("Solo puedes editar rutinas propias personalizadas o copiadas.");
        }

        var rutina = await rutinaRepository.UpdateRutinaAsync(idRutina, request);
        return rutina is null ? NotFound("Rutina no encontrada.") : Ok(rutina);
    }
}
