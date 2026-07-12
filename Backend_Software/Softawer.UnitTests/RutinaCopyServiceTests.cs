using Softawer.Models;
using Softawer.Services;

namespace Softawer.UnitTests;

public class RutinaCopyServiceTests
{
    [Fact]
    public void CrearCopiaEditable_ConRutinaPredefinida_AsignaPropietarioYOrigen()
    {
        // Arrange
        var service = new RutinaCopyService();
        var origen = CrearRutinaOrigen();

        // Act
        var copia = service.CrearCopiaEditable(origen, 99);

        // Assert
        Assert.Equal(99, copia.IdUsuario);
        Assert.Equal(origen.IdRutina, copia.IdRutinaOrigen);
        Assert.Equal("copiada", copia.TipoRutina);
        Assert.False(copia.EsPublica);
        Assert.Equal("Mi Fuerza principiante", copia.Nombre);
    }

    [Fact]
    public void CrearCopiaEditable_AlModificarEjercicioCopiado_NoModificaRutinaOriginal()
    {
        // Arrange
        var service = new RutinaCopyService();
        var origen = CrearRutinaOrigen();

        // Act
        var copia = service.CrearCopiaEditable(origen, 99);
        copia.Ejercicios[0].Series = 8;
        copia.Ejercicios[0].Notas = "Modificada";

        // Assert
        Assert.NotSame(origen.Ejercicios, copia.Ejercicios);
        Assert.NotSame(origen.Ejercicios[0], copia.Ejercicios[0]);
        Assert.Equal(4, origen.Ejercicios[0].Series);
        Assert.Equal("Original", origen.Ejercicios[0].Notas);
    }

    private static RutinaDetalleResponse CrearRutinaOrigen() => new()
    {
        IdRutina = 12,
        Nombre = "Fuerza principiante",
        Descripcion = "Rutina original",
        TipoRutina = "predefinida",
        Objetivo = "ganar_fuerza",
        Dificultad = "principiante",
        EsPublica = true,
        Ejercicios =
        [
            new RutinaEjercicio
            {
                IdEjercicio = 3,
                Dia = "Lunes",
                Orden = 1,
                Series = 4,
                Repeticiones = 10,
                NombreEjercicio = "Curl",
                Notas = "Original"
            }
        ]
    };
}
