using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class MercadoLibreController : ControllerBase
{
    private readonly IMercadoLibreService _service;

    public MercadoLibreController(IMercadoLibreService service)
    {
        _service = service;
    }

    [Authorize(Roles = "viewer,admin,superadmin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [Authorize(Roles = "viewer,admin,superadmin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [Authorize(Roles = "admin,superadmin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MercadoLibreManualDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "admin,superadmin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] MercadoLibreManualDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [Authorize(Roles = "admin,superadmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [Authorize(Roles = "viewer,admin,superadmin")]
    [HttpGet("daily")]
    public async Task<ActionResult<List<DailySalesDto>>> GetDailyStats([FromQuery] int year, [FromQuery] int month)
    {
        var result = await _service.GetSimulatedDailyStatsAsync(year, month);
        return Ok(result);
    }
}
