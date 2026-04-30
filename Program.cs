using MySqlConnector;
using Softawer.Data;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "FrontendClient";

var connectionString = builder.Configuration.GetConnectionString("GymDb")
    ?? throw new InvalidOperationException("Connection string 'GymDb' was not configured.");

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200"];

builder.Services.AddControllers();
builder.Services.AddSingleton(new MySqlDataSource(connectionString));
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<DatabaseHealthRepository>();

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
app.MapControllers();

app.Run();
