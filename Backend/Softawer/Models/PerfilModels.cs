namespace Softawer.Models;

public class PerfilUsuario
{
    public int IdPerfil { get; set; }
    public int IdUsuario { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public string? Sexo { get; set; }
    public decimal? AlturaCm { get; set; }
    public decimal? PesoKg { get; set; }
    public string? Objetivo { get; set; }
    public string? NivelActividad { get; set; }
    public string? Alias { get; set; }
    public string? AvatarUrl { get; set; }
    public string? TelefonoTrabajo { get; set; }
    public string? EmailTrabajo { get; set; }
    public string? SitioPersonal { get; set; }
    public string? Twitter { get; set; }
    public DateTime FechaActualizacion { get; set; }
}

public class HistorialImc
{
    public int IdImc { get; set; }
    public int IdUsuario { get; set; }
    public decimal AlturaCm { get; set; }
    public decimal PesoKg { get; set; }
    public decimal Imc { get; set; }
    public string CategoriaImc { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
}

public class ActualizarPerfilImcRequest
{
    public DateOnly? FechaNacimiento { get; set; }
    public string? Sexo { get; set; }
    public decimal AlturaCm { get; set; }
    public decimal PesoKg { get; set; }
    public string? Objetivo { get; set; }
    public string? NivelActividad { get; set; }
}

public class ActualizarInformacionPublicaRequest
{
    public string? Alias { get; set; }
    public string? AvatarUrl { get; set; }
    public string? TelefonoTrabajo { get; set; }
    public string? EmailTrabajo { get; set; }
    public string? SitioPersonal { get; set; }
    public string? Twitter { get; set; }
}

public class ImcRecommendationResponse
{
    public int IdUsuario { get; set; }
    public decimal Imc { get; set; }
    public string CategoriaImc { get; set; } = string.Empty;
    public PerfilUsuario Perfil { get; set; } = new();
    public List<RutinaResumenResponse> RutinasRecomendadas { get; set; } = [];
}
