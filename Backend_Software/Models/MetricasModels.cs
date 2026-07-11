namespace Softawer.Models;

public class Metrica
{
    public int IdMetrica { get; set; }
    public int IdUsuario { get; set; }
    public string Ejercicio { get; set; } = string.Empty;
    public decimal PesoKg { get; set; }
    public DateOnly Fecha { get; set; }
    public string? Notas { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class CrearMetricaRequest
{
    public int IdUsuario { get; set; }
    public string Ejercicio { get; set; } = string.Empty;
    public decimal PesoKg { get; set; }
    public DateOnly Fecha { get; set; }
    public string? Notas { get; set; }
}
