using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/plantillas-whatsapp")]
[Authorize] // Requiere autenticación
public class PlantillaWhatsAppController : ControllerBase
{
    private readonly IPlantillaWhatsAppService _service;

    public PlantillaWhatsAppController(IPlantillaWhatsAppService service)
    {
        _service = service;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Obtener todas las plantillas activas")]
    public async Task<ActionResult<List<PlantillaWhatsAppDto>>> GetAll()
    {
        var plantillas = await _service.GetAllActivasAsync();
        return Ok(plantillas);
    }

    [HttpGet("categorias")]
    [SwaggerOperation(Summary = "Obtener plantillas agrupadas por categoría")]
    public async Task<ActionResult<Dictionary<string, List<PlantillaWhatsAppDto>>>> GetPorCategorias()
    {
        var plantillasPorCategoria = await _service.GetPorCategoriasAsync();
        return Ok(plantillasPorCategoria);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obtener plantilla por ID")]
    public async Task<ActionResult<PlantillaWhatsAppDto>> GetById(int id)
    {
        var plantilla = await _service.GetByIdAsync(id);
        if (plantilla == null) return NotFound();
        return Ok(plantilla);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Crear nueva plantilla")]
    [Authorize(Roles = "admin,superadmin")] // Solo admins pueden crear
    public async Task<ActionResult<PlantillaWhatsAppDto>> Create([FromBody] CrearPlantillaWhatsAppDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        var username = User.Identity?.Name ?? "Sistema";
        var plantilla = await _service.CrearAsync(dto, username);
        
        return CreatedAtAction(nameof(GetById), new { id = plantilla.Id }, plantilla);
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Actualizar plantilla")]
    [Authorize(Roles = "admin,superadmin")] // Solo admins pueden editar
    public async Task<ActionResult<PlantillaWhatsAppDto>> Update(int id, [FromBody] ActualizarPlantillaWhatsAppDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        var plantilla = await _service.ActualizarAsync(id, dto);
        if (plantilla == null) return NotFound();
        
        return Ok(plantilla);
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Desactivar plantilla")]
    [Authorize(Roles = "admin,superadmin")] // Solo admins pueden desactivar
    public async Task<IActionResult> Delete(int id)
    {
        var eliminada = await _service.DesactivarAsync(id);
        if (!eliminada) return NotFound();
        
        return NoContent();
    }
}