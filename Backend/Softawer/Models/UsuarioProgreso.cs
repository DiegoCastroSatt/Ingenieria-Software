namespace Softawer.Models;

public class UsuarioProgreso
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public int IdRutina { get; set; }
    public DateTime Fecha { get; set; }

    public int PorcentajeCompletado { get; set; } // 0 - 100
    public int SesionesCompletadas { get; set; }
    public int TotalSesiones { get; set; }

    public string Observaciones { get; set; } = string.Empty;
}