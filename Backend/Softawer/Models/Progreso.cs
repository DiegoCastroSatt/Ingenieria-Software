namespace Softawer.Models;

public class Progreso
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public int IdRutina { get; set; }
    public decimal Peso { get; set; }
    public int Repeticiones { get; set; }
    public DateTime Fecha { get; set; }
    public string RutinaNombre { get; set; } = string.Empty;
}
public class ProgresoResponse
{
    public int SesionesPagadas { get; set; }
    public int SesionesImpagas { get; set; }
    public int SesionesRealizadas { get; set; }
    public int SesionesRestantes { get; set; }
}