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

        var reservasActivas = await reservaRepository.GetReservasActivasMaquinaAsync(request.IdMaquina, request.FechaReserva);
        var error = reservaPolicyService.ValidarReserva(maquina, reservasActivas, request);
        if (error is not null)
        {
            return Conflict(error);
        }

        var reserva = await reservaRepository.CreateReservaAsync(request);
        return CreatedAtAction(nameof(GetReservasUsuario), new { idUsuario = request.IdUsuario }, reserva);
    }
}
