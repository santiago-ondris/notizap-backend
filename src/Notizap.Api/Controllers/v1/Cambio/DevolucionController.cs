using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Notizap.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class DevolucionController : ControllerBase
{
    private readonly IDevolucionService _devolucionService;

    public DevolucionController(IDevolucionService devolucionService)
    {
        _devolucionService = devolucionService;
    }

    [HttpPost]
    [Authorize(Roles = "admin,superadmin")]
    [SwaggerOperation(Summary = "Crea un nuevo registro de devolución.")]
    public async Task<ActionResult<int>> Crear([FromBody] CreateDevolucionDto dto)
    {
        var id = await _devolucionService.CrearDevolucionAsync(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id }, id);
    }

    [HttpGet]
    [Authorize(Roles = "viewer,admin,superadmin")]
    [SwaggerOperation(Summary = "Devuelve la lista de todas las devoluciones registradas.")]
    public async Task<ActionResult<List<DevolucionDto>>> ObtenerTodas()
    {
        var devoluciones = await _devolucionService.ObtenerTodasAsync();
        return Ok(devoluciones);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "viewer,admin,superadmin")]
    [SwaggerOperation(Summary = "Devuelve una devolución específica por ID.")]
    public async Task<ActionResult<DevolucionDto>> ObtenerPorId(int id)
    {
        var devolucion = await _devolucionService.ObtenerPorIdAsync(id);
        if (devolucion == null) return NotFound();
        return Ok(devolucion);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,superadmin")]
    [SwaggerOperation(Summary = "Actualiza una devolución existente por ID.")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] DevolucionDto dto)
    {
        var actualizado = await _devolucionService.ActualizarDevolucionAsync(id, dto);
        return actualizado ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,superadmin")]
    [SwaggerOperation(Summary = "Elimina una devolución por ID.")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var eliminado = await _devolucionService.EliminarDevolucionAsync(id);
        return eliminado ? NoContent() : NotFound();
    }
}
