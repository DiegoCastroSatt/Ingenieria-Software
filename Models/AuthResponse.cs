namespace Softawer.Models;

public class AuthResponse
{
    public AuthResponse(string message, AuthUserResponse user)
    {
        Message = message;
        User = user;
    }

    public string Message { get; }
    public AuthUserResponse User { get; }
}
