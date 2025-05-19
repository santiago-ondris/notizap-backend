using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Notizap.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class CambioController : ControllerBase
{
    private readonly ICambioService _cambioService;

    public CambioController(ICambioService cambioService)
    {
        _cambioService = cambioService;
    }

    [HttpPost]
    [Authorize(Roles = "admin,superadmin")]
    [SwaggerOperation(Summary = "Crea un nuevo registro de cambio")]
    public async Task<ActionResult<int>> Crear([FromBody] CreateCambioDto dto)
    {
        var id = await _cambioService.CrearCambioAsync(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id }, id);
    }

    [HttpGet]
    [Authorize(Roles = "viewer,admin,superadmin")]
    [SwaggerOperation(Summary = "Devuelve la lista de todos los cambios registrados")]
    public async Task<ActionResult<List<CambioDto>>> ObtenerTodos()
    {
        var cambios = await _cambioService.ObtenerTodosAsync();
        return Ok(cambios);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "viewer,admin,superadmin")]
    [SwaggerOperation(Summary = "Devuelve un cambio específico por ID")]
    public async Task<ActionResult<CambioDto>> ObtenerPorId(int id)
    {
        var cambio = await _cambioService.ObtenerPorIdAsync(id);
        if (cambio == null) return NotFound();
        return Ok(cambio);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,superadmin")]
    [SwaggerOperation(Summary = "Actualiza un cambio existente por ID")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] CambioDto dto)
    {
        var actualizado = await _cambioService.ActualizarCambioAsync(id, dto);
        return actualizado ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,superadmin")]
    [SwaggerOperation(Summary = "Elimina un cambio existente por ID")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var eliminado = await _cambioService.EliminarCambioAsync(id);
        return eliminado ? NoContent() : NotFound();
    }
}
