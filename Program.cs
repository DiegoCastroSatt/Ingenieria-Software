using System.Text;
using MySqlConnector;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Softawer.Data;
using Softawer.Services;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "FrontendClient";

var connectionString = builder.Configuration.GetConnectionString("GymDb")
    ?? throw new InvalidOperationException("Connection string 'GymDb' was not configured.");

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200"];

var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey was not configured.");

// Servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(new MySqlDataSource(connectionString));

// Repositorios
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<PerfilUsuarioRepository>();
builder.Services.AddScoped<DatabaseHealthRepository>();
builder.Services.AddScoped<DatabaseSchemaInitializer>();
builder.Services.AddScoped<CatalogoRepository>();
builder.Services.AddScoped<RutinaRepository>();
builder.Services.AddScoped<ReservaRepository>();
builder.Services.AddScoped<SesionEntrenamientoRepository>();

// Servicios de negocio
builder.Services.AddSingleton<PasswordHashService>();
builder.Services.AddSingleton<ImcService>();
builder.Services.AddSingleton<ReservaPolicyService>();
builder.Services.AddSingleton<RutinaCopyService>();
builder.Services.AddSingleton<SesionEntrenamientoService>();
builder.Services.AddSingleton<JwtService>();

// Autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = "Softawer",
            ValidateAudience = true,
            ValidAudience = "GymClient",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// CORS
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

using (var scope = app.Services.CreateScope())
{
    try
    {
        var schemaInitializer = scope.ServiceProvider.GetRequiredService<DatabaseSchemaInitializer>();
        await schemaInitializer.EnsureUsuarioMaquinaFavoritaAsync();
        await schemaInitializer.EnsureReservaCancelacionesAsync();
    }
    catch (Exception exception)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(exception, "No se pudieron asegurar las tablas auxiliares.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
