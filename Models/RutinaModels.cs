namespace Softawer.Models;

public class Rutina
{
    public int IdRutina { get; set; }
    public int? IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string TipoRutina { get; set; } = string.Empty;
    public string Objetivo { get; set; } = string.Empty;
    public string? CategoriaImc { get; set; }
    public string Dificultad { get; set; } = string.Empty;
    public bool EsPublica { get; set; }
    public int? IdRutinaOrigen { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
}

public class RutinaEjercicio
{
    public int IdRutinaEjercicio { get; set; }
    public int IdRutina { get; set; }
    public int IdEjercicio { get; set; }
    public string Dia { get; set; } = string.Empty;
    public int Orden { get; set; }
    public int? Series { get; set; }
    public int? Repeticiones { get; set; }
    public int? DuracionMinutos { get; set; }
    public int? DescansoSegundos { get; set; }
    public string? Notas { get; set; }
    public string NombreEjercicio { get; set; } = string.Empty;
    public string DescripcionEjercicio { get; set; } = string.Empty;
    public string GrupoMuscular { get; set; } = string.Empty;
}

public class RutinaResumenResponse
{
    public int IdRutina { get; set; }
    public int? IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string TipoRutina { get; set; } = string.Empty;
    public string Objetivo { get; set; } = string.Empty;
    public string? CategoriaImc { get; set; }
    public string Dificultad { get; set; } = string.Empty;
    public bool EsPublica { get; set; }
    public int? IdRutinaOrigen { get; set; }
}

public class RutinaDetalleResponse : RutinaResumenResponse
{
    public List<RutinaEjercicio> Ejercicios { get; set; } = [];
}

public class RutinaEjercicioRequest
{
    public int IdEjercicio { get; set; }
    public string Dia { get; set; } = "Lunes";
    public int Orden { get; set; }
    public int? Series { get; set; }
    public int? Repeticiones { get; set; }
    public int? DuracionMinutos { get; set; }
    public int? DescansoSegundos { get; set; }
    public string? Notas { get; set; }
}

public class CrearRutinaRequest
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Objetivo { get; set; } = string.Empty;
    public string Dificultad { get; set; } = "principiante";
    public bool EsPublica { get; set; }
    public List<RutinaEjercicioRequest> Ejercicios { get; set; } = [];
}

public class ActualizarRutinaRequest
{
    public int IdUsuario { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public string? Objetivo { get; set; }
    public string? Dificultad { get; set; }
    public bool? EsPublica { get; set; }
    public List<RutinaEjercicioRequest> Ejercicios { get; set; } = [];
}

public class CopiarRutinaRequest
{
    public int IdUsuario { get; set; }
    public bool ActivarRutina { get; set; } = true;
}

public class UsuarioRutina
{
    public int IdUsuarioRutina { get; set; }
    public int IdUsuario { get; set; }
    public int IdRutina { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaAsignacion { get; set; }
}
