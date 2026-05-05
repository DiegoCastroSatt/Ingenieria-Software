using Softawer.Models;

namespace Softawer.Services;

public class ReservaPolicyService
{
    public bool TieneTraslape(TimeOnly inicioExistente, TimeOnly finExistente, TimeOnly inicioNuevo, TimeOnly finNuevo)
    {
        return inicioExistente < finNuevo && finExistente > inicioNuevo;
    }

    public bool PermiteReservaSegunEstado(string estadoMaquina)
    {
        return estadoMaquina is "disponible" or "ocupada";
    }

    public string? ValidarReserva(Maquina maquina, IReadOnlyList<Reserva> reservasActivas, CrearReservaRequest request)
    {
        if (!PermiteReservaSegunEstado(maquina.Estado))
        {
            return "La maquina no puede reservarse porque esta en mantencion o fuera de servicio.";
        }

        if (request.HoraInicio >= request.HoraFin)
        {
            return "La hora de inicio debe ser menor que la hora de termino.";
        }

        var conflicto = reservasActivas.Any(reserva =>
            reserva.Estado == "activa" &&
            TieneTraslape(reserva.HoraInicio, reserva.HoraFin, request.HoraInicio, request.HoraFin));

        if (conflicto)
        {
            return "Ya existe una reserva activa que se cruza con el horario solicitado.";
        }

        return null;
    }
}
