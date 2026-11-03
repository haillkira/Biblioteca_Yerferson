using Biblioteca_Yerferson.Models;
using Biblioteca_Yerferson.Services;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca_Yerferson.Controllers;

[ApiController]
[Route("api/libros")]
public sealed class LibrosController(
    LibrosService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        return Ok(await service.GetListAsync());
    }

    [HttpGet("{id:int}/disponibilidad")]
    public async Task<IActionResult> GetDisponibilidad(int id)
    {
        int disponibles = await service.GetDisponiblesAsync(id);
        return Ok(new { LibroId = id, Disponibles = disponibles });
    }

    [HttpPost]
    public async Task<IActionResult> Save(
        [FromBody] Libro libro)
    {
        var creado = await service.SaveAsync(libro);
        return StatusCode(StatusCodes.Status201Created, creado);
    }
}