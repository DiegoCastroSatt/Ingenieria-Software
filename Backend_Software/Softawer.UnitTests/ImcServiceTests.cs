using Softawer.Services;

namespace Softawer.UnitTests;

public class ImcServiceTests
{
    [Fact]
    public void CalcularImc_ConAlturaYPesoValidos_RetornaValorRedondeado()
    {
        // Arrange
        var service = new ImcService();

        // Act
        var resultado = service.CalcularImc(175m, 70m);

        // Assert
        Assert.Equal(22.86m, resultado);
    }

    [Theory]
    [InlineData(18.49, "bajo_peso")]
    [InlineData(18.50, "normal")]
    [InlineData(25.00, "sobrepeso")]
    [InlineData(30.00, "obesidad")]
    public void ClasificarImc_EnLimitesDeCategoria_RetornaCategoriaEsperada(double imc, string categoriaEsperada)
    {
        // Arrange
        var service = new ImcService();

        // Act
        var resultado = service.ClasificarImc((decimal)imc);

        // Assert
        Assert.Equal(categoriaEsperada, resultado);
    }

    [Fact]
    public void CalcularImc_ConAlturaCero_LanzaArgumentOutOfRangeException()
    {
        // Arrange
        var service = new ImcService();

        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => service.CalcularImc(0m, 70m));

        // Assert
        Assert.Equal("alturaCm", exception.ParamName);
    }
}
