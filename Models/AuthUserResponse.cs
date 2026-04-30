namespace Softawer.Models;

public class AuthUserResponse
{
    public AuthUserResponse(int id, string nombre, string correo)
    {
        Id = id;
        Nombre = nombre;
        Correo = correo;
    }

    public int Id { get; }
    public string Nombre { get; }
    public string Correo { get; }
}
