using Softawer.Services;

namespace Softawer.UnitTests;

public class PasswordHashServiceTests
{
    [Fact]
    public void HashPassword_ConPasswordValido_GeneraHashVerificableDistintoAlTextoPlano()
    {
        // Arrange
        var service = new PasswordHashService();
        const string password = "clave-segura";

        // Act
        var hash = service.HashPassword(password);

        // Assert
        Assert.NotEqual(password, hash);
        Assert.StartsWith("PBKDF2$", hash);
        Assert.True(service.VerifyPassword(password, hash));
    }

    [Fact]
    public void VerifyPassword_ConPasswordIncorrecto_RetornaFalse()
    {
        // Arrange
        var service = new PasswordHashService();
        var hash = service.HashPassword("password-correcto");

        // Act
        var resultado = service.VerifyPassword("password-incorrecto", hash);

        // Assert
        Assert.False(resultado);
    }
}
