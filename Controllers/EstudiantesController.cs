using Biblioteca_Yerferson.Models;
using Biblioteca_Yerferson.Services;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca_Yerferson.Controllers;

[ApiController]
[Route("api/estudiantes")]
public sealed class EstudiantesController(
    EstudiantesService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        return Ok(await service.GetListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Save(
        [FromBody] Estudiante estudiante)
    {
        var creado = await service.SaveAsync(estudiante);
        return StatusCode(StatusCodes.Status201Created, creado);
    }
}