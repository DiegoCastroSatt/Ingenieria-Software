using Softawer.Models;

namespace Softawer.Services;

public class SesionEntrenamientoService
{
    public SesionEntrenamiento CrearSesion(IniciarSesionRequest request)
    {
        return new SesionEntrenamiento
        {
            IdUsuario = request.IdUsuario,
            IdRutina = request.IdRutina,
            FechaInicio = DateTime.UtcNow,
            Estado = "en_progreso",
            Notas = request.Notas
        };
    }

    public DetalleSesionEntrenamiento CrearDetalle(int idSesion, AgregarDetalleSesionRequest request)
    {
        return new DetalleSesionEntrenamiento
        {
            IdSesion = idSesion,
            IdEjercicio = request.IdEjercicio,
            IdMaquina = request.IdMaquina,
            IdReserva = request.IdReserva,
            SeriesRealizadas = request.SeriesRealizadas,
            RepeticionesRealizadas = request.RepeticionesRealizadas,
            PesoUsadoKg = request.PesoUsadoKg,
            DuracionMinutos = request.DuracionMinutos,
            EsfuerzoPercibido = request.EsfuerzoPercibido,
            Notas = request.Notas,
            FechaCreacion = DateTime.UtcNow
        };
    }

    public void CompletarSesion(SesionEntrenamiento sesion, CompletarSesionRequest request)
    {
        if (sesion.Estado != "en_progreso")
        {
            throw new InvalidOperationException("Solo se pueden completar sesiones en progreso.");
        }

        sesion.Estado = "completada";
        sesion.FechaFin = DateTime.UtcNow;
        sesion.Notas = request.Notas ?? sesion.Notas;
    }
}
