namespace Softawer.Models;

public class LoginRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Rut { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public int Edad {get; set; }
    public string Nacionalidad { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Rol { get; set; } = "usuario";
}

public class AuthUserResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public int Edad {get; set; }
    public string Nacionalidad { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Message { get; set; } = string.Empty;
    public AuthUserResponse User { get; set; } = new();
    public string Token { get; set; } = string.Empty;
}
