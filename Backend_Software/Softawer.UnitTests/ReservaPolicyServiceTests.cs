using Softawer.Models;
using Softawer.Services;

namespace Softawer.UnitTests;

public class ReservaPolicyServiceTests
{
    [Theory]
    [InlineData(9, 10, 10, 11, false)]
    [InlineData(10, 11, 9, 10, false)]
    [InlineData(10, 11, 10.5, 11.5, true)]
    [InlineData(10, 12, 11, 13, true)]
    public void TieneTraslape_ConDistintosHorarios_RetornaResultadoEsperado(
        double inicioExistente,
        double finExistente,
        double inicioNuevo,
        double finNuevo,
        bool esperado)
    {
        // Arrange
        var service = new ReservaPolicyService();

        // Act
        var resultado = service.TieneTraslape(
            CrearHora(inicioExistente),
            CrearHora(finExistente),
            CrearHora(inicioNuevo),
            CrearHora(finNuevo));

        // Assert
        Assert.Equal(esperado, resultado);
    }

    [Theory]
    [InlineData("disponible", true)]
    [InlineData("ocupada", true)]
    [InlineData("mantencion", false)]
    [InlineData("fuera_servicio", false)]
    public void PermiteReservaSegunEstado_ConEstadoConocido_RetornaResultadoEsperado(string estado, bool esperado)
    {
        // Arrange
        var service = new ReservaPolicyService();

        // Act
        var resultado = service.PermiteReservaSegunEstado(estado);

        // Assert
        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void ValidarReserva_ConHorarioTraslapado_RetornaMensajeDeConflicto()
    {
        // Arrange
        var service = new ReservaPolicyService();
        var fechaActual = new DateOnly(2026, 7, 11);
        var maquina = new Maquina { Estado = "disponible" };
        var reservasActivas = new List<Reserva>
        {
            new() { Estado = "activa", HoraInicio = new TimeOnly(10, 0), HoraFin = new TimeOnly(11, 0) }
        };
        var request = new CrearReservaRequest
        {
            FechaReserva = fechaActual,
            HoraInicio = new TimeOnly(10, 30),
            HoraFin = new TimeOnly(11, 30)
        };

        // Act
        var error = service.ValidarReserva(maquina, reservasActivas, request, fechaActual);

        // Assert
        Assert.Equal("Ya existe una reserva activa que se cruza con el horario solicitado.", error);
    }

    [Fact]
    public void ValidarReserva_ConFechaPasada_RetornaMensajeDeError()
    {
        // Arrange
        var service = new ReservaPolicyService();
        var fechaActual = new DateOnly(2026, 7, 11);
        var request = new CrearReservaRequest
        {
            FechaReserva = fechaActual.AddDays(-1),
            HoraInicio = new TimeOnly(10, 0),
            HoraFin = new TimeOnly(11, 0)
        };

        // Act
        var error = service.ValidarReserva(new Maquina { Estado = "disponible" }, [], request, fechaActual);

        // Assert
        Assert.Equal("No se pueden crear reservas en fechas anteriores a hoy.", error);
    }

    [Fact]
    public void ValidarReserva_ConHoraFinIgualAInicio_RetornaMensajeDeError()
    {
        // Arrange
        var service = new ReservaPolicyService();
        var fechaActual = new DateOnly(2026, 7, 11);
        var request = new CrearReservaRequest
        {
            FechaReserva = fechaActual,
            HoraInicio = new TimeOnly(10, 0),
            HoraFin = new TimeOnly(10, 0)
        };

        // Act
        var error = service.ValidarReserva(new Maquina { Estado = "disponible" }, [], request, fechaActual);

        // Assert
        Assert.Equal("La hora de inicio debe ser menor que la hora de termino.", error);
    }

    [Fact]
    public void ValidarCancelacion_ConReservaPropiaActivaDeHoy_PermiteCancelar()
    {
        // Arrange
        var service = new ReservaPolicyService();
        var fechaActual = new DateOnly(2026, 7, 11);
        var reserva = new Reserva { IdUsuario = 7, Estado = "activa", FechaReserva = fechaActual };

        // Act
        var error = service.ValidarCancelacion(reserva, 7, fechaActual);

        // Assert
        Assert.Null(error);
    }

    [Fact]
    public void ValidarCancelacion_ConReservaDeOtroUsuario_RetornaMensajeDeError()
    {
        // Arrange
        var service = new ReservaPolicyService();
        var fechaActual = new DateOnly(2026, 7, 11);
        var reserva = new Reserva { IdUsuario = 7, Estado = "activa", FechaReserva = fechaActual };

        // Act
        var error = service.ValidarCancelacion(reserva, 8, fechaActual);

        // Assert
        Assert.Equal("Solo puedes cancelar tus propias reservas.", error);
    }

    private static TimeOnly CrearHora(double valor)
    {
        var horas = (int)valor;
        var minutos = (int)Math.Round((valor - horas) * 60);
        return new TimeOnly(horas, minutos);
    }
}
