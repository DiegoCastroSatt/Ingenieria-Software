using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Softawer.Data;
using Softawer.Models;

namespace Softawer.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(UsuarioRepository usuarioRepository) : ControllerBase
{
    [HttpPost("login")]
    [HttpPost("/login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest login)
    {
        var validationError = ValidateLogin(login);
        if (validationError is not null)
        {
            return validationError;
        }

        try
        {
            var usuario = await usuarioRepository.AuthenticateAsync(login.Nombre.Trim(), login.Password.Trim());
            if (usuario is null)
            {
                return BadRequest("Usuario o contrasena incorrectos.");
            }

            return Ok(new AuthResponse("Login correcto", new AuthUserResponse(usuario.Id, usuario.Nombre, usuario.Correo)));
        }
        catch (Exception exception)
        {
            return Problem(
                detail: exception.Message,
                title: "Login failed",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("register")]
    [HttpPost("/register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var validationError = ValidateRegister(request);
        if (validationError is not null)
        {
            return validationError;
        }

        try
        {
            var exists = await usuarioRepository.ExistsByCorreoOrRutAsync(request.Correo.Trim(), request.Rut.Trim());
            if (exists)
            {
                return BadRequest("Ya existe un usuario con ese correo o RUT.");
            }

            var nuevoUsuario = await usuarioRepository.CreateUsuarioAsync(new Usuario
            {
                Nombre = request.Nombre.Trim(),
                Rut = request.Rut.Trim(),
                Correo = request.Correo.Trim(),
                Contrasena = request.Password.Trim()
            });

            return Ok(new AuthResponse(
                "Usuario registrado correctamente.",
                new AuthUserResponse(nuevoUsuario.Id, nuevoUsuario.Nombre, nuevoUsuario.Correo)));
        }
        catch (Exception exception)
        {
            return Problem(
                detail: exception.Message,
                title: "Register failed",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private ActionResult? ValidateLogin(LoginRequest login)
    {
        if (string.IsNullOrWhiteSpace(login.Nombre) || string.IsNullOrWhiteSpace(login.Password))
        {
            return BadRequest("Debes ingresar usuario y contrasena.");
        }

        return null;
    }

    private ActionResult? ValidateRegister(RegisterRequest user)
    {
        if (string.IsNullOrWhiteSpace(user.Nombre) ||
            string.IsNullOrWhiteSpace(user.Rut) ||
            string.IsNullOrWhiteSpace(user.Correo) ||
            string.IsNullOrWhiteSpace(user.Password))
        {
            return BadRequest("Todos los campos son obligatorios.");
        }

        var emailValidator = new EmailAddressAttribute();
        if (!emailValidator.IsValid(user.Correo))
        {
            return BadRequest("El correo no tiene un formato valido.");
        }

        if (user.Password.Trim().Length < 4)
        {
            return BadRequest("La contrasena debe tener al menos 4 caracteres.");
        }

        return null;
    }
}
