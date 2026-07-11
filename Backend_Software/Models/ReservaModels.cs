namespace Softawer.Models;

public class Reserva
{
    public int IdReserva { get; set; }
    public int IdUsuario { get; set; }
    public int IdMaquina { get; set; }
    public string NombreMaquina { get; set; } = string.Empty;
    public DateOnly FechaReserva { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
}

public class CrearReservaRequest
{
    public int IdUsuario { get; set; }
    public int IdMaquina { get; set; }
    public DateOnly FechaReserva { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
}

public class CancelarReservaRequest
{
    public int IdUsuario { get; set; }
}
