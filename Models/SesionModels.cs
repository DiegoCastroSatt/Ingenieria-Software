namespace Softawer.Models;

public class SesionEntrenamiento
{
    public int IdSesion { get; set; }
    public int IdUsuario { get; set; }
    public int? IdRutina { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Notas { get; set; }
}

public class DetalleSesionEntrenamiento
{
    public int IdDetalle { get; set; }
    public int IdSesion { get; set; }
    public int IdEjercicio { get; set; }
    public string NombreEjercicio { get; set; } = string.Empty;
    public int? IdMaquina { get; set; }
    public string? NombreMaquina { get; set; }
    public int? IdReserva { get; set; }
    public int? SeriesRealizadas { get; set; }
    public int? RepeticionesRealizadas { get; set; }
    public decimal? PesoUsadoKg { get; set; }
    public int? DuracionMinutos { get; set; }
    public int? EsfuerzoPercibido { get; set; }
    public string? Notas { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class Progreso
{
    public int IdProgreso { get; set; }
    public int IdUsuario { get; set; }
    public int? IdRutina { get; set; }
    public DateOnly Fecha { get; set; }
    public decimal PorcentajeCompletado { get; set; }
    public decimal? CaloriasEstimadas { get; set; }
    public int? TiempoTotalMinutos { get; set; }
    public string? Observacion { get; set; }
}

public class IniciarSesionRequest
{
    public int IdUsuario { get; set; }
    public int? IdRutina { get; set; }
    public string? Notas { get; set; }
}

public class AgregarDetalleSesionRequest
{
    public int IdEjercicio { get; set; }
    public int? IdMaquina { get; set; }
    public int? IdReserva { get; set; }
    public int? SeriesRealizadas { get; set; }
    public int? RepeticionesRealizadas { get; set; }
    public decimal? PesoUsadoKg { get; set; }
    public int? DuracionMinutos { get; set; }
    public int? EsfuerzoPercibido { get; set; }
    public string? Notas { get; set; }
}

public class CompletarSesionRequest
{
    public decimal PorcentajeCompletado { get; set; }
    public decimal? CaloriasEstimadas { get; set; }
    public int? TiempoTotalMinutos { get; set; }
    public string? Observacion { get; set; }
    public string? Notas { get; set; }
}

public class SesionHistorialResponse
{
    public SesionEntrenamiento Sesion { get; set; } = new();
    public RutinaResumenResponse? Rutina { get; set; }
    public Progreso? Progreso { get; set; }
    public List<DetalleSesionEntrenamiento> Detalles { get; set; } = [];
}
