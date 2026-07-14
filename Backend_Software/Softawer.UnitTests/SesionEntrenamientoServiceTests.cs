using Softawer.Models;
using Softawer.Services;

namespace Softawer.UnitTests;

public class SesionEntrenamientoServiceTests
{
    [Fact]
    public void CrearSesion_ConRequestValido_IniciaSesionEnProgreso()
    {
        // Arrange
        var service = new SesionEntrenamientoService();
        var request = new IniciarSesionRequest { IdUsuario = 1, IdRutina = 2, Notas = "Inicio de prueba" };
        var antes = DateTime.UtcNow;

        // Act
        var sesion = service.CrearSesion(request);
        var despues = DateTime.UtcNow;

        // Assert
        Assert.Equal(1, sesion.IdUsuario);
        Assert.Equal(2, sesion.IdRutina);
        Assert.Equal("en_progreso", sesion.Estado);
        Assert.InRange(sesion.FechaInicio, antes, despues);
    }

    [Fact]
    public void CompletarSesion_ConSesionEnProgreso_MarcaSesionComoCompletada()
    {
        // Arrange
        var service = new SesionEntrenamientoService();
        var sesion = new SesionEntrenamiento { Estado = "en_progreso", Notas = "Inicial" };

        // Act
        service.CompletarSesion(sesion, new CompletarSesionRequest { Notas = "Final" });

        // Assert
        Assert.Equal("completada", sesion.Estado);
        Assert.NotNull(sesion.FechaFin);
        Assert.Equal("Final", sesion.Notas);
    }

    [Fact]
    public void CompletarSesion_ConSesionYaCompletada_LanzaInvalidOperationException()
    {
        // Arrange
        var service = new SesionEntrenamientoService();
        var sesion = new SesionEntrenamiento { Estado = "completada" };

        // Act
        var exception = Assert.Throws<InvalidOperationException>(
            () => service.CompletarSesion(sesion, new CompletarSesionRequest()));

        // Assert
        Assert.Equal("Solo se pueden completar sesiones en progreso.", exception.Message);
    }
}
