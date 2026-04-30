namespace Softawer.Models;

public class RegisterRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Rut { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
