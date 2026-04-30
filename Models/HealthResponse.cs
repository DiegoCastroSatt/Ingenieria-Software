namespace Softawer.Models;

public class HealthResponse
{
    public HealthResponse(string status, string message)
    {
        Status = status;
        Message = message;
    }

    public string Status { get; }
    public string Message { get; }
}
