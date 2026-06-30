using Microsoft.AspNetCore.Mvc;
using Softawer.Data;
using Softawer.Models;

namespace Softawer.Controllers;

[ApiController]
[Route("api/catalogo")]
public class CatalogoController(CatalogoRepository catalogoRepository) : ControllerBase
{
    [HttpGet("maquinas")]
    [ProducesResponseType(typeof(IReadOnlyList<Maquina>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Maquina>>> GetMaquinas()
    {
        return Ok(await catalogoRepository.ListMaquinasAsync());
    }

    [HttpGet("maquinas/favoritas/{idUsuario:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<Maquina>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Maquina>>> GetMaquinasFavoritas(int idUsuario)
    {
        return Ok(await catalogoRepository.ListMaquinasFavoritasAsync(idUsuario));
    }

    [HttpPost("maquinas/{idMaquina:int}/favorita")]
    [ProducesResponseType(typeof(IReadOnlyList<Maquina>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<Maquina>>> AddMaquinaFavorita(int idMaquina, MaquinaFavoritaRequest request)
    {
        var added = await catalogoRepository.AddMaquinaFavoritaAsync(request.IdUsuario, idMaquina);
        if (!added)
        {
            return ApiError.NotFound(this, "El usuario o la maquina no existen.");
        }

        return Ok(await catalogoRepository.ListMaquinasFavoritasAsync(request.IdUsuario));
    }

    [HttpDelete("maquinas/{idMaquina:int}/favorita")]
    [ProducesResponseType(typeof(IReadOnlyList<Maquina>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<Maquina>>> RemoveMaquinaFavorita(int idMaquina, [FromQuery] int idUsuario)
    {
        var removed = await catalogoRepository.RemoveMaquinaFavoritaAsync(idUsuario, idMaquina);
        if (!removed)
        {
            return ApiError.NotFound(this, "El usuario o la maquina no existen.");
        }

        return Ok(await catalogoRepository.ListMaquinasFavoritasAsync(idUsuario));
    }

    [HttpGet("ejercicios")]
    [ProducesResponseType(typeof(IReadOnlyList<Ejercicio>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Ejercicio>>> GetEjercicios()
    {
        return Ok(await catalogoRepository.ListEjerciciosAsync());
    }
}
