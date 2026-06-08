using Microsoft.AspNetCore.Mvc;
using Softawer.Data;
using Softawer.Models;
using Softawer.Services;

namespace Softawer.Controllers;

[ApiController]
[Route("api/perfiles")]
public class PerfilController(
    UsuarioRepository usuarioRepository,
    PerfilUsuarioRepository perfilUsuarioRepository,
    RutinaRepository rutinaRepository,
    ImcService imcService) : ControllerBase
{
    [HttpGet("{idUsuario:int}")]
    public async Task<ActionResult<PerfilUsuario>> GetPerfil(int idUsuario)
    {
        var usuario = await usuarioRepository.GetUsuarioAsync(idUsuario);
        if (usuario is null)
        {
            return NotFound("Usuario no encontrado.");
        }

        var perfil = await perfilUsuarioRepository.GetPerfilAsync(idUsuario);
        return perfil is null ? NotFound("Perfil no encontrado.") : Ok(perfil);
    }

    [HttpGet("{idUsuario:int}/historial-imc")]
    public async Task<ActionResult<IReadOnlyList<HistorialImc>>> GetHistorialImc(int idUsuario)
    {
        var usuario = await usuarioRepository.GetUsuarioAsync(idUsuario);
        if (usuario is null)
        {
            return NotFound("Usuario no encontrado.");
        }

        return Ok(await perfilUsuarioRepository.ListHistorialImcUsuarioAsync(idUsuario));
    }

    [HttpPost("{idUsuario:int}/imc")]
    public async Task<ActionResult<ImcRecommendationResponse>> ActualizarPerfilImc(int idUsuario, [FromBody] ActualizarPerfilImcRequest request)
    {
        var usuario = await usuarioRepository.GetUsuarioAsync(idUsuario);
        if (usuario is null)
        {
            return NotFound("Usuario no encontrado.");
        }

        var perfil = await perfilUsuarioRepository.UpsertPerfilAsync(idUsuario, request);
        var imc = imcService.CalcularImc(request.AlturaCm, request.PesoKg);
        var categoriaImc = imcService.ClasificarImc(imc);
        await perfilUsuarioRepository.AddHistorialImcAsync(idUsuario, request.AlturaCm, request.PesoKg, imc, categoriaImc);

        return Ok(new ImcRecommendationResponse
        {
            IdUsuario = idUsuario,
            Imc = imc,
            CategoriaImc = categoriaImc,
            Perfil = perfil,
            RutinasRecomendadas = (await rutinaRepository.ListRutinasRecomendadasAsync(categoriaImc)).ToList()
        });
    }

    [HttpGet("{idUsuario:int}/recomendaciones")]
    public async Task<ActionResult<IReadOnlyList<RutinaResumenResponse>>> GetRecomendaciones(int idUsuario)
    {
        var categoriaImc = await perfilUsuarioRepository.GetCategoriaImcActualAsync(idUsuario);
        if (string.IsNullOrWhiteSpace(categoriaImc))
        {
            return NotFound("El usuario no tiene historial IMC registrado.");
        }

        return Ok(await rutinaRepository.ListRutinasRecomendadasAsync(categoriaImc));
    }

    [HttpPut("{idUsuario:int}/informacion-publica")]
    public async Task<ActionResult<PerfilUsuario>> ActualizarInformacionPublica(int idUsuario, [FromBody] ActualizarInformacionPublicaRequest request)
    {
        var usuario = await usuarioRepository.GetUsuarioAsync(idUsuario);
        if (usuario is null)
        {
            return NotFound("Usuario no encontrado.");
        }

        var perfil = await perfilUsuarioRepository.UpsertInformacionPublicaAsync(idUsuario, request);
        
        return Ok(perfil);
    }
}
