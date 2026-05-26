namespace Softawer.Models;

public class Maquina
{
    public int IdMaquina { get; set; }
    public int IdTipoMaquina { get; set; }
    public string TipoMaquina { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string MusculosObjetivo { get; set; } = string.Empty;
    public string ImagenUrl { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public class Ejercicio
{
    public int IdEjercicio { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string GrupoMuscular { get; set; } = string.Empty;
    public string Dificultad { get; set; } = string.Empty;
    public int? IdMaquina { get; set; }
    public string? NombreMaquina { get; set; }
}
