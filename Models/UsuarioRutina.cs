namespace Softawer.Models;

public class UsuarioRutina
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public int IdRutina { get; set; }
    public DateTime FechaInicio { get; set; }
    public string RutinaNombre { get; set; } = string.Empty;
    public string RutinaDescripcion { get; set; } = string.Empty;
}
