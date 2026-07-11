using Microsoft.AspNetCore.Mvc;
using Softawer.Data;
using Softawer.Models;
using Softawer.Services;

namespace Softawer.Controllers;

[ApiController]
[Route("api/sesiones")]
public class SesionesEntrenamientoController(
    UsuarioRepository usuarioRepository,
    CatalogoRepository catalogoRepository,
    SesionEntrenamientoRepository sesionEntrenamientoRepository,
    SesionEntrenamientoService sesionEntrenamientoService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(SesionEntrenamiento), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SesionEntrenamiento>> IniciarSesion([FromBody] IniciarSesionRequest request)
    {
        if (await usuarioRepository.GetUsuarioAsync(request.IdUsuario) is null)
        {
            return ApiError.NotFound(this, "Usuario no encontrado.");
        }

        var sesion = sesionEntrenamientoService.CrearSesion(request);
        return Ok(await sesionEntrenamientoRepository.CreateSesionAsync(sesion));
    }

    [HttpPost("{idSesion:int}/detalles")]
    [ProducesResponseType(typeof(DetalleSesionEntrenamiento), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DetalleSesionEntrenamiento>> AgregarDetalle(int idSesion, [FromBody] AgregarDetalleSesionRequest request)
    {
        var sesion = await sesionEntrenamientoRepository.GetSesionAsync(idSesion);
        if (sesion is null)
        {
            return ApiError.NotFound(this, "Sesion no encontrada.");
        }

        if (sesion.Estado != "en_progreso")
        {
            return ApiError.Conflict(this, "Solo se pueden agregar ejercicios a sesiones en progreso.");
        }

        if (!(await catalogoRepository.ListEjerciciosAsync()).Any(ejercicio => ejercicio.IdEjercicio == request.IdEjercicio))
        {
            return ApiError.NotFound(this, "Ejercicio no encontrado.");
        }

        var detalle = sesionEntrenamientoService.CrearDetalle(idSesion, request);
        return Ok(await sesionEntrenamientoRepository.AddDetalleAsync(detalle));
    }

    [HttpPost("{idSesion:int}/completar")]
    [ProducesResponseType(typeof(SesionEntrenamiento), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SesionEntrenamiento>> CompletarSesion(int idSesion, [FromBody] CompletarSesionRequest request)
    {
        var sesion = await sesionEntrenamientoRepository.GetSesionAsync(idSesion);
        if (sesion is null)
        {
            return ApiError.NotFound(this, "Sesion no encontrada.");
        }

        try
        {
            sesionEntrenamientoService.CompletarSesion(sesion, request);
        }
        catch (InvalidOperationException exception)
        {
            return ApiError.Conflict(this, exception.Message);
        }

        await sesionEntrenamientoRepository.CompleteSesionAsync(sesion, request);
        return Ok(sesion);
    }

    [HttpGet("usuario/{idUsuario:int}/historial")]
    [ProducesResponseType(typeof(IReadOnlyList<SesionHistorialResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SesionHistorialResponse>>> GetHistorial(int idUsuario)
    {
        return Ok(await sesionEntrenamientoRepository.GetHistorialUsuarioAsync(idUsuario));
    }
}
