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
    ImcService imcService,
    IWebHostEnvironment environment) : ControllerBase
{
    private static readonly HashSet<string> AllowedAvatarExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".webp"
    };

    [HttpGet("{idUsuario:int}")]
    [ProducesResponseType(typeof(PerfilUsuario), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PerfilUsuario>> GetPerfil(int idUsuario)
    {
        var usuario = await usuarioRepository.GetUsuarioAsync(idUsuario);
        if (usuario is null)
        {
            return ApiError.NotFound(this, "Usuario no encontrado.");
        }

        var perfil = await perfilUsuarioRepository.GetPerfilAsync(idUsuario);
        return perfil is null ? ApiError.NotFound(this, "Perfil no encontrado.") : Ok(perfil);
    }

    [HttpGet("{idUsuario:int}/historial-imc")]
    [ProducesResponseType(typeof(IReadOnlyList<HistorialImc>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<HistorialImc>>> GetHistorialImc(int idUsuario)
    {
        var usuario = await usuarioRepository.GetUsuarioAsync(idUsuario);
        if (usuario is null)
        {
            return ApiError.NotFound(this, "Usuario no encontrado.");
        }

        return Ok(await perfilUsuarioRepository.ListHistorialImcUsuarioAsync(idUsuario));
    }

    [HttpPost("{idUsuario:int}/imc")]
    [ProducesResponseType(typeof(ImcRecommendationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImcRecommendationResponse>> ActualizarPerfilImc(int idUsuario, [FromBody] ActualizarPerfilImcRequest request)
    {
        var usuario = await usuarioRepository.GetUsuarioAsync(idUsuario);
        if (usuario is null)
        {
            return ApiError.NotFound(this, "Usuario no encontrado.");
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
    [ProducesResponseType(typeof(IReadOnlyList<RutinaResumenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<RutinaResumenResponse>>> GetRecomendaciones(int idUsuario)
    {
        var categoriaImc = await perfilUsuarioRepository.GetCategoriaImcActualAsync(idUsuario);
        if (string.IsNullOrWhiteSpace(categoriaImc))
        {
            return ApiError.NotFound(this, "El usuario no tiene historial IMC registrado.");
        }

        return Ok(await rutinaRepository.ListRutinasRecomendadasAsync(categoriaImc));
    }

    [HttpPut("{idUsuario:int}/informacion-publica")]
    [ProducesResponseType(typeof(PerfilUsuario), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PerfilUsuario>> ActualizarInformacionPublica(int idUsuario, [FromBody] ActualizarInformacionPublicaRequest request)
    {
        var usuario = await usuarioRepository.GetUsuarioAsync(idUsuario);
        if (usuario is null)
        {
            return ApiError.NotFound(this, "Usuario no encontrado.");
        }

        var perfil = await perfilUsuarioRepository.UpsertInformacionPublicaAsync(idUsuario, request);

        return Ok(perfil);
    }

    [HttpPost("{idUsuario:int}/avatar")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(3 * 1024 * 1024)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> SubirAvatar(int idUsuario, IFormFile avatar)
    {
        var usuario = await usuarioRepository.GetUsuarioAsync(idUsuario);
        if (usuario is null)
        {
            return ApiError.NotFound(this, "Usuario no encontrado.");
        }

        if (avatar.Length == 0 || string.IsNullOrWhiteSpace(avatar.ContentType) || !avatar.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return ApiError.BadRequest(this, "Debes subir una imagen valida.");
        }

        var extension = Path.GetExtension(avatar.FileName);
        if (!AllowedAvatarExtensions.Contains(extension))
        {
            return ApiError.BadRequest(this, "Formato no permitido. Usa JPG, PNG, GIF o WEBP.");
        }

        var uploadsDirectory = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", "avatars");
        Directory.CreateDirectory(uploadsDirectory);

        var fileName = $"{idUsuario}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadsDirectory, fileName);

        await using (var stream = System.IO.File.Create(filePath))
        {
            await avatar.CopyToAsync(stream);
        }

        var avatarUrl = $"{Request.Scheme}://{Request.Host}/uploads/avatars/{fileName}";
        return Ok(new { avatarUrl });
    }
}
