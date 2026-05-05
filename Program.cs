using MySqlConnector;
using Softawer.Data;
using Softawer.Services;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "FrontendClient";

var connectionString = builder.Configuration.GetConnectionString("GymDb")
    ?? throw new InvalidOperationException("Connection string 'GymDb' was not configured.");

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200"];

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton(new MySqlDataSource(connectionString));

builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<PerfilUsuarioRepository>();
builder.Services.AddScoped<DatabaseHealthRepository>();
builder.Services.AddScoped<CatalogoRepository>();
builder.Services.AddScoped<RutinaRepository>();
builder.Services.AddScoped<ReservaRepository>();
builder.Services.AddScoped<SesionEntrenamientoRepository>();

builder.Services.AddSingleton<PasswordHashService>();
builder.Services.AddSingleton<ImcService>();
builder.Services.AddSingleton<ReservaPolicyService>();
builder.Services.AddSingleton<RutinaCopyService>();
builder.Services.AddSingleton<SesionEntrenamientoService>();

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
