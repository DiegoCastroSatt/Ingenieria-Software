namespace Softawer.Models;

public class Usuario
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Rut { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string ContrasenaHash { get; set; } = string.Empty;
    public string Nacionalidad { get; set; } = string.Empty;
    public string Edad { get; set; } = string.Empty;
    public string Rol { get; set; } = "usuario";
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
}
