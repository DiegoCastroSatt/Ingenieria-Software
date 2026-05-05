using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Softawer.Data;
using Softawer.Models;
using Softawer.Services;

namespace Softawer.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(UsuarioRepository usuarioRepository, PasswordHashService passwordHashService) : ControllerBase
{
    [HttpPost("login")]
    [HttpPost("/login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest login)
    {
        if (string.IsNullOrWhiteSpace(login.Nombre) || string.IsNullOrWhiteSpace(login.Password))
        {
            return BadRequest("Debes ingresar usuario y contrasena.");
        }

        var usuario = await usuarioRepository.GetByLoginIdentifierAsync(login.Nombre.Trim());
        if (usuario is null || !passwordHashService.VerifyPassword(login.Password.Trim(), usuario.ContrasenaHash))
        {
            return BadRequest("Usuario o contrasena incorrectos.");
        }

        return Ok(new AuthResponse
        {
            Message = "Login correcto",
            User = new AuthUserResponse
            {
                Id = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Rol = usuario.Rol
            }
        });
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

        if (await usuarioRepository.ExistsByCorreoOrRutAsync(request.Correo.Trim(), request.Rut.Trim()))
        {
            return BadRequest("Ya existe un usuario con ese correo o RUT.");
        }

        var usuario = await usuarioRepository.CreateUsuarioAsync(new Usuario
        {
            Nombre = request.Nombre.Trim(),
            Rut = request.Rut.Trim(),
            Correo = request.Correo.Trim(),
            Rol = string.IsNullOrWhiteSpace(request.Rol) ? "usuario" : request.Rol.Trim(),
            ContrasenaHash = passwordHashService.HashPassword(request.Password.Trim())
        });

        return Ok(new AuthResponse
        {
            Message = "Usuario registrado correctamente.",
            User = new AuthUserResponse
            {
                Id = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Rol = usuario.Rol
            }
        });
    }

    private ActionResult? ValidateRegister(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre) ||
            string.IsNullOrWhiteSpace(request.Rut) ||
            string.IsNullOrWhiteSpace(request.Correo) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Todos los campos son obligatorios.");
        }

        if (!new EmailAddressAttribute().IsValid(request.Correo))
        {
            return BadRequest("El correo no tiene un formato valido.");
        }

        if (request.Password.Trim().Length < 4)
        {
            return BadRequest("La contrasena debe tener al menos 4 caracteres.");
        }

        return null;
    }
}
