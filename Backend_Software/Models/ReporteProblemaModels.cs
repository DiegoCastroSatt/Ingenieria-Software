namespace Softawer.Models;

public class ReporteProblema
{
    public int IdReporte { get; set; }
    public int IdUsuario { get; set; }
    public int? IdMaquina { get; set; }
    public string? NombreMaquina { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public string Estado { get; set; } = "pendiente";
    public DateTime FechaActualizacion { get; set; }
}

public class CrearReporteProblemaRequest
{
    public int IdUsuario { get; set; }
    public int? IdMaquina { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}
