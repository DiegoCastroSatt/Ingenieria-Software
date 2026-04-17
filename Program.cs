//using MySql.Data.MySqlClient;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("AllowAll");

string connectionString = "server=localhost;database=Gym_Software;user=gymuser;password=1234;";

app.MapPost("/login", (UserLogin login) =>
{
    using (MySqlConnection conn = new MySqlConnection(connectionString))
    {
        conn.Open();

        string query = "SELECT * FROM usuarios WHERE nombre=@user AND contrasena=@pass";

        using (MySqlCommand cmd = new MySqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@user", login.Nombre);
            cmd.Parameters.AddWithValue("@pass", login.Password);

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    return Results.Ok("Login correcto");
                }
            }
        }
    }

    return Results.BadRequest("Usuario o contraseña incorrectos");
});


app.MapPost("/register", (UserRegister user) =>
{
    using (MySqlConnection conn = new MySqlConnection(connectionString))
    {
        conn.Open();

        // verificar si ya existe
        string checkQuery = "SELECT * FROM usuarios WHERE correo=@correo";

        using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
        {
            checkCmd.Parameters.AddWithValue("@correo", user.Correo);

            using (var reader = checkCmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    return Results.BadRequest("El usuario ya existe");
                }
            }
        }

        string insertQuery = "INSERT INTO usuarios (nombre, rut, correo, contrasena) VALUES (@nombre, @rut, @correo, @pass)";

        using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
        {
            cmd.Parameters.AddWithValue("@nombre", user.Nombre);
            cmd.Parameters.AddWithValue("@rut", user.Rut);
            cmd.Parameters.AddWithValue("@correo", user.Correo);
            cmd.Parameters.AddWithValue("@pass", user.Password);

            cmd.ExecuteNonQuery();
        }
    }

    return Results.Ok("Usuario registrado correctamente");
});

app.Run();

public class UserLogin
{
    public string Nombre { get; set; } = "";
    public string Password { get; set; } = "";
}

public class UserRegister
{
    public string Nombre { get; set; } = "";
    public string Rut { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Password { get; set; } = "";
}