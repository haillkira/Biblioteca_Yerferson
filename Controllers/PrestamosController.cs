using Biblioteca_Yerferson.Models;
using Biblioteca_Yerferson.Services;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca_Yerferson.Controllers;

[ApiController]
[Route("api/prestamos")]
public sealed class PrestamosController(
    PrestamosService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        return Ok(await service.GetListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Save(
        [FromBody] CrearPrestamo solicitud)
    {
        var creado = await service.SaveAsync(solicitud);
        return StatusCode(StatusCodes.Status201Created, creado);
    }
}