using System.ComponentModel.DataAnnotations;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "FrontendClient";

var connectionString = builder.Configuration.GetConnectionString("GymDb")
    ?? throw new InvalidOperationException("Connection string 'GymDb' was not configured.");

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors(CorsPolicyName);

app.MapGet("/api/health", async () =>
{
    try
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand("SELECT 1", connection);
        await command.ExecuteScalarAsync();

        return Results.Ok(new HealthResponse("ok", "API and database connection are available."));
    }
    catch (Exception exception)
    {
        return Results.Problem(
            detail: exception.Message,
            title: "Database connection failed",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapPost("/api/auth/login", LoginAsync);
app.MapPost("/api/auth/register", RegisterAsync);

// Legacy routes kept for compatibility with existing frontend code.
app.MapPost("/login", LoginAsync);
app.MapPost("/register", RegisterAsync);

app.Run();

async Task<IResult> LoginAsync(LoginRequest login)
{
    var validationError = ValidateLogin(login);
    if (validationError is not null)
    {
        return validationError;
    }

    try
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        const string query = """
            SELECT id, nombre, correo
            FROM usuarios
            WHERE nombre = @nombre AND contrasena = @password
            LIMIT 1;
            """;

        await using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@nombre", login.Nombre.Trim());
        command.Parameters.AddWithValue("@password", login.Password.Trim());

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return Results.BadRequest("Usuario o contrasena incorrectos.");
        }

        var user = new AuthUserResponse(
            reader.GetInt32("id"),
            reader.GetString("nombre"),
            reader.GetString("correo"));

        return Results.Ok(new AuthResponse("Login correcto", user));
    }
    catch (Exception exception)
    {
        return Results.Problem(
            detail: exception.Message,
            title: "Login failed",
            statusCode: StatusCodes.Status500InternalServerError);
    }
}

async Task<IResult> RegisterAsync(RegisterRequest user)
{
    var validationError = ValidateRegister(user);
    if (validationError is not null)
    {
        return validationError;
    }

    try
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        const string duplicateQuery = """
            SELECT COUNT(*)
            FROM usuarios
            WHERE correo = @correo OR rut = @rut;
            """;

        await using (var duplicateCommand = new MySqlCommand(duplicateQuery, connection))
        {
            duplicateCommand.Parameters.AddWithValue("@correo", user.Correo.Trim());
            duplicateCommand.Parameters.AddWithValue("@rut", user.Rut.Trim());

            var existingUsers = Convert.ToInt32(await duplicateCommand.ExecuteScalarAsync());
            if (existingUsers > 0)
            {
                return Results.BadRequest("Ya existe un usuario con ese correo o RUT.");
            }
        }

        const string insertQuery = """
            INSERT INTO usuarios (nombre, rut, correo, contrasena)
            VALUES (@nombre, @rut, @correo, @password);
            SELECT LAST_INSERT_ID();
            """;

        await using var insertCommand = new MySqlCommand(insertQuery, connection);
        insertCommand.Parameters.AddWithValue("@nombre", user.Nombre.Trim());
        insertCommand.Parameters.AddWithValue("@rut", user.Rut.Trim());
        insertCommand.Parameters.AddWithValue("@correo", user.Correo.Trim());
        insertCommand.Parameters.AddWithValue("@password", user.Password.Trim());

        var newUserId = Convert.ToInt32(await insertCommand.ExecuteScalarAsync());
        var response = new AuthResponse(
            "Usuario registrado correctamente.",
            new AuthUserResponse(newUserId, user.Nombre.Trim(), user.Correo.Trim()));

        return Results.Ok(response);
    }
    catch (Exception exception)
    {
        return Results.Problem(
            detail: exception.Message,
            title: "Register failed",
            statusCode: StatusCodes.Status500InternalServerError);
    }
}

static IResult? ValidateLogin(LoginRequest login)
{
    if (string.IsNullOrWhiteSpace(login.Nombre) || string.IsNullOrWhiteSpace(login.Password))
    {
        return Results.BadRequest("Debes ingresar usuario y contrasena.");
    }

    return null;
}

static IResult? ValidateRegister(RegisterRequest user)
{
    if (string.IsNullOrWhiteSpace(user.Nombre) ||
        string.IsNullOrWhiteSpace(user.Rut) ||
        string.IsNullOrWhiteSpace(user.Correo) ||
        string.IsNullOrWhiteSpace(user.Password))
    {
        return Results.BadRequest("Todos los campos son obligatorios.");
    }

    var emailValidator = new EmailAddressAttribute();
    if (!emailValidator.IsValid(user.Correo))
    {
        return Results.BadRequest("El correo no tiene un formato valido.");
    }

    if (user.Password.Trim().Length < 4)
    {
        return Results.BadRequest("La contrasena debe tener al menos 4 caracteres.");
    }

    return null;
}

public record LoginRequest(string Nombre, string Password);

public record RegisterRequest(string Nombre, string Rut, string Correo, string Password);

public record AuthUserResponse(int Id, string Nombre, string Correo);

public record AuthResponse(string Message, AuthUserResponse User);

public record HealthResponse(string Status, string Message);
