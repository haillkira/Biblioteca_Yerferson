using Biblioteca_Yerferson.Models;
using Biblioteca_Yerferson.Services;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca_Yerferson.Controllers;

[ApiController]
[Route("api/devoluciones")]
public sealed class DevolucionesController(
    DevolucionesService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        return Ok(await service.GetListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Save(
        [FromBody] CrearDevolucion solicitud)
    {
        var creado = await service.SaveAsync(solicitud);
        return StatusCode(StatusCodes.Status201Created, creado);
    }
}