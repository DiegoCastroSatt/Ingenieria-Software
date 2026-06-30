using Softawer.Services;

namespace Softawer.Tests;

public class ImcServiceTests
{
    // se usa mucho SUT, significa Subject Under Testing (Sujeto de Pruebas)
    private readonly ImcService _sut = new();

    [Fact]
    public void CalcularImc_ValoresNormales_RetornaImcCorrecto()
    {
        // Arrange
        decimal alturaCm = 170m;   // 1.70 m
        decimal pesoKg   = 70m;

        // Act
        var imc = _sut.CalcularImc(alturaCm, pesoKg);

        // Assert
        // IMC = 70 / (1.70 ^ 2) = 70 / 2.89 ≈ 24.22
        Assert.Equal(24.22m, imc);
    }

    [Fact]
    public void CalcularImc_AlturaEnCero_LanzaArgumentOutOfRangeException()
    {
        // Arrange
        decimal alturaCm = 0m;
        decimal pesoKg   = 70m;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.CalcularImc(alturaCm, pesoKg));
    }

    //aca se usan los inlineData, sirven para probar diferentes casos
    [Theory]
    [InlineData(16.0,  "bajo_peso")]
    [InlineData(18.49, "bajo_peso")]
    [InlineData(18.5,  "normal")]
    [InlineData(22.0,  "normal")]
    [InlineData(24.99, "normal")]
    [InlineData(25.0,  "sobrepeso")]
    [InlineData(27.5,  "sobrepeso")]
    [InlineData(30.0,  "obesidad")]
    [InlineData(35.0,  "obesidad")]
    public void ClasificarImc_Categoria_RetornaEtiquetaEsperada(double imcDouble, string categoriaEsperada)
    {
        // Arrange
        var imc = (decimal)imcDouble;

        // Act
        var resultado = _sut.ClasificarImc(imc);

        // Assert
        Assert.Equal(categoriaEsperada, resultado);
    }
}
