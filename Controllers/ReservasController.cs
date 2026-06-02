using Microsoft.AspNetCore.Mvc;
using Softawer.Data;
using Softawer.Models;
using Softawer.Services;

namespace Softawer.Controllers;

[ApiController]
[Route("api/reservas")]
public class ReservasController(
    UsuarioRepository usuarioRepository,
    CatalogoRepository catalogoRepository,
    ReservaRepository reservaRepository,
    ReservaPolicyService reservaPolicyService) : ControllerBase
{
    [HttpGet("usuario/{idUsuario:int}")]
    public async Task<ActionResult<IReadOnlyList<Reserva>>> GetReservasUsuario(int idUsuario)
    {
        return Ok(await reservaRepository.ListReservasUsuarioAsync(idUsuario));
    }

    [HttpGet("activas")]
    public async Task<ActionResult<IReadOnlyList<Reserva>>> GetReservasActivas([FromQuery] DateOnly fechaReserva)
    {
        return Ok(await reservaRepository.ListReservasActivasPorFechaAsync(fechaReserva));
    }

    [HttpPost]
    public async Task<ActionResult<Reserva>> CrearReserva([FromBody] CrearReservaRequest request)
    {
        if (await usuarioRepository.GetUsuarioAsync(request.IdUsuario) is null)
        {
            return NotFound("Usuario no encontrado.");
        }

        var maquina = await catalogoRepository.GetMaquinaAsync(request.IdMaquina);
        if (maquina is null)
        {
            return NotFound("Maquina no encontrada.");
        }

        var fechaActual = DateOnly.FromDateTime(DateTime.Today);
        var reservasActivas = await reservaRepository.GetReservasActivasMaquinaAsync(request.IdMaquina, request.FechaReserva);
        var error = reservaPolicyService.ValidarReserva(maquina, reservasActivas, request, fechaActual);
        if (error is not null)
        {
            return Conflict(error);
        }

        var reserva = await reservaRepository.CreateReservaAsync(request);
        return CreatedAtAction(nameof(GetReservasUsuario), new { idUsuario = request.IdUsuario }, reserva);
    }

    [HttpPost("{idReserva:int}/cancelar")]
    public async Task<ActionResult<Reserva>> CancelarReserva(int idReserva, [FromBody] CancelarReservaRequest request)
    {
        if (request.IdUsuario <= 0)
        {
            return BadRequest("El usuario es obligatorio.");
        }

        var reserva = await reservaRepository.GetReservaAsync(idReserva);
        if (reserva is null)
        {
            return NotFound("Reserva no encontrada.");
        }

        var error = reservaPolicyService.ValidarCancelacion(reserva, request.IdUsuario, DateOnly.FromDateTime(DateTime.Today));
        if (error is not null)
        {
            return Conflict(error);
        }

        return Ok(await reservaRepository.CancelReservaAsync(reserva));
    }
}
