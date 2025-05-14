using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v{version:apiVersion}/envios")]
[ApiVersion("1.0")]
public class EnviosController : ControllerBase
{
    private readonly IEnvioService _envioService;

    public EnviosController(IEnvioService envioService)
    {
        _envioService = envioService;
    }

    [HttpGet("mensual")]
    [Authorize(Roles = "viewer,admin,superadmin")]
    public async Task<ActionResult<List<EnvioDiarioDto>>> GetMensual([FromQuery] int year, [FromQuery] int month)
    {
        return await _envioService.ObtenerPorMesAsync(year, month);
    }

    [HttpGet("fecha")]
    [Authorize(Roles = "viewer,admin,superadmin")]
    public async Task<ActionResult<EnvioDiarioDto>> GetPorFecha([FromQuery] DateTime fecha)
    {
        var envio = await _envioService.ObtenerPorFechaAsync(fecha);
        if (envio == null) return NotFound();
        return envio;
    }

    [HttpPost]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<IActionResult> Post([FromBody] CreateEnvioDiarioDto dto)
    {
        await _envioService.CrearOActualizarAsync(dto);
        return Ok("Registro creado o actualizado");
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<IActionResult> Put(int id, [FromBody] CreateEnvioDiarioDto dto)
    {
        await _envioService.EditarAsync(id, dto);
        return Ok("Registro editado");
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _envioService.EliminarAsync(id);
        return Ok("Registro eliminado");
    }

    [HttpGet("resumen")]
    [Authorize(Roles = "viewer,admin,superadmin")]
    public async Task<ActionResult<EnvioResumenMensualDto>> GetResumen([FromQuery] int year, [FromQuery] int month)
    {
        var resumen = await _envioService.ObtenerResumenMensualAsync(year, month);
        return Ok(resumen);
    }
}
