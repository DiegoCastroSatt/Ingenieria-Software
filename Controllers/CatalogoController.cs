using Microsoft.AspNetCore.Mvc;
using Softawer.Data;
using Softawer.Models;

namespace Softawer.Controllers;

[ApiController]
[Route("api/catalogo")]
public class CatalogoController(CatalogoRepository catalogoRepository) : ControllerBase
{
    [HttpGet("maquinas")]
    public async Task<ActionResult<IReadOnlyList<Maquina>>> GetMaquinas()
    {
        return Ok(await catalogoRepository.ListMaquinasAsync());
    }

    [HttpGet("ejercicios")]
    public async Task<ActionResult<IReadOnlyList<Ejercicio>>> GetEjercicios()
    {
        return Ok(await catalogoRepository.ListEjerciciosAsync());
    }
}
