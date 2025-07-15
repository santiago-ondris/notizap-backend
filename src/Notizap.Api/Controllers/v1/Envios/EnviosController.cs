using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Route("api/v{version:apiVersion}/envios")]
[ApiVersion("1.0")]
public class EnviosController : ControllerBase
{
    private readonly IEnvioService _envioService;
    private readonly ILogger<EnviosController> _logger;

    public EnviosController(IEnvioService envioService, ILogger<EnviosController> logger)
    {
        _envioService = envioService;
        _logger = logger;
    }

    // --- Ejemplo logging en POST ---
    [HttpPost]
    [Authorize(Roles = "admin,superadmin")]
    [SwaggerOperation(Summary = "Crear un envio")]
    public async Task<IActionResult> Post([FromBody] CreateEnvioDiarioDto dto)
    {
        var user = User.Identity?.Name ?? "anonymous";
        _logger.LogInformation("Usuario {User} solicita crear envío para fecha {Fecha}", user, dto.Fecha);

        try
        {
            await _envioService.CrearOActualizarAsync(dto);
            _logger.LogInformation("Envío creado/actualizado correctamente para fecha {Fecha} por usuario {User}", dto.Fecha, user);
            return Ok("Registro creado o actualizado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear envío para fecha {Fecha} por usuario {User}", dto.Fecha, user);
            return StatusCode(500, "Error al crear/actualizar el envío");
        }
    }

    // --- Ejemplo logging en PUT ---
    [HttpPut("{id}")]
    [Authorize(Roles = "admin,superadmin")]
    [SwaggerOperation(Summary = "Actualizar un envio diario")]
    public async Task<IActionResult> Put(int id, [FromBody] CreateEnvioDiarioDto dto)
    {
        var user = User.Identity?.Name ?? "anonymous";
        _logger.LogInformation("Usuario {User} solicita editar envío Id {Id}", user, id);

        try
        {
            await _envioService.EditarAsync(id, dto);
            _logger.LogInformation("Envío Id {Id} editado correctamente por usuario {User}", id, user);
            return Ok("Registro editado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar envío Id {Id} por usuario {User}", id, user);
            return StatusCode(500, "Error al editar el envío");
        }
    }

    // --- Ejemplo logging en DELETE ---
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,superadmin")]
    [SwaggerOperation(Summary = "Eliminar un envio diario")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = User.Identity?.Name ?? "anonymous";
        _logger.LogInformation("Usuario {User} solicita eliminar envío Id {Id}", user, id);

        try
        {
            await _envioService.EliminarAsync(id);
            _logger.LogInformation("Envío Id {Id} eliminado correctamente por usuario {User}", id, user);
            return Ok("Registro eliminado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar envío Id {Id} por usuario {User}", id, user);
            return StatusCode(500, "Error al eliminar el envío");
        }
    }

    // --- Logging opcional en lote (acá es útil) ---
    [HttpPost("lote")]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<ActionResult<ResultadoLoteDto>> GuardarLote([FromBody] GuardarEnviosLoteDto request)
    {
        var user = User.Identity?.Name ?? "anonymous";
        _logger.LogInformation("Usuario {User} solicita guardar lote de envíos. Cantidad: {Cantidad}", user, request.Envios.Count);

        if (!request.Envios.Any())
        {
            _logger.LogWarning("Usuario {User} intentó guardar lote vacío", user);
            return BadRequest("No se enviaron envíos para guardar");
        }

        try
        {
            var resultado = await _envioService.CrearOActualizarLoteAsync(request.Envios);

            if (resultado.TodosExitosos)
            {
                _logger.LogInformation("Lote de envíos guardado correctamente por usuario {User}", user);
                return Ok(resultado);
            }
            else
            {
                _logger.LogWarning("Lote con fallos ({Fallidos} fallidos) procesado por usuario {User}", resultado.Fallidos, user);
                return BadRequest(resultado);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico al guardar lote de envíos por usuario {User}", user);
            return StatusCode(500, new ResultadoLoteDto
            {
                Fallidos = request.Envios.Count,
                Mensaje = $"Error crítico del servidor: {ex.Message}. Intenta guardando celda por celda para encontrar el error específico.",
                Errores = new List<string> { ex.Message }
            });
        }
    }

    // --- GETs suelen no loguearse salvo casos especiales ---
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

    [HttpGet("resumen")]
    [Authorize(Roles = "viewer,admin,superadmin")]
    [SwaggerOperation(Summary = "Obtener resumen mensual de envios")]
    public async Task<ActionResult<EnvioResumenMensualDto>> GetResumen([FromQuery] int year, [FromQuery] int month)
    {
        var resumen = await _envioService.ObtenerResumenMensualAsync(year, month);
        return Ok(resumen);
    }
}
