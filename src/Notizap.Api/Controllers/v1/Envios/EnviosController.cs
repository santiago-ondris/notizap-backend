using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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
    [SwaggerOperation(Summary = "Obtener envios mensuales")]
    public async Task<ActionResult<List<EnvioDiarioDto>>> GetMensual([FromQuery] int year, [FromQuery] int month)
    {
        return await _envioService.ObtenerPorMesAsync(year, month);
    }

    [HttpGet("fecha")]
    [Authorize(Roles = "viewer,admin,superadmin")]
    [SwaggerOperation(Summary = "Obtener envios por dia")]
    public async Task<ActionResult<EnvioDiarioDto>> GetPorFecha([FromQuery] DateTime fecha)
    {
        var fechaUtc = DateTime.SpecifyKind(fecha, DateTimeKind.Utc);
        var envio = await _envioService.ObtenerPorFechaAsync(fechaUtc);
        if (envio == null) return NotFound();
        return envio;
    }

    [HttpPost]
    [Authorize(Roles = "admin,superadmin")]
    [SwaggerOperation(Summary = "Crear un envio")]
    public async Task<IActionResult> Post([FromBody] CreateEnvioDiarioDto dto)
    {
        await _envioService.CrearOActualizarAsync(dto);
        return Ok("Registro creado o actualizado");
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,superadmin")]
    [SwaggerOperation(Summary = "Actualizar un envio diario")]
    public async Task<IActionResult> Put(int id, [FromBody] CreateEnvioDiarioDto dto)
    {
        await _envioService.EditarAsync(id, dto);
        return Ok("Registro editado");
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,superadmin")]
    [SwaggerOperation(Summary = "Eliminar un envio diario")]
    public async Task<IActionResult> Delete(int id)
    {
        await _envioService.EliminarAsync(id);
        return Ok("Registro eliminado");
    }

    [HttpGet("resumen")]
    [Authorize(Roles = "viewer,admin,superadmin")]
    [SwaggerOperation(Summary = "Obtener resumen mensual de envios")]
    public async Task<ActionResult<EnvioResumenMensualDto>> GetResumen([FromQuery] int year, [FromQuery] int month)
    {
        var resumen = await _envioService.ObtenerResumenMensualAsync(year, month);
        return Ok(resumen);
    }

    [HttpPost("lote")]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<ActionResult<ResultadoLoteDto>> GuardarLote([FromBody] GuardarEnviosLoteDto request)
    {
        if (!request.Envios.Any())
            return BadRequest("No se enviaron envíos para guardar");

        try
        {
            var resultado = await _envioService.CrearOActualizarLoteAsync(request.Envios);
            
            if (resultado.TodosExitosos)
            {
                return Ok(resultado);
            }
            else
            {
                return BadRequest(resultado);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResultadoLoteDto 
            { 
                Fallidos = request.Envios.Count,
                Mensaje = $"Error crítico del servidor: {ex.Message}. Intenta guardando celda por celda para encontrar el error específico.",
                Errores = new List<string> { ex.Message }
            });
        }
    }
}
