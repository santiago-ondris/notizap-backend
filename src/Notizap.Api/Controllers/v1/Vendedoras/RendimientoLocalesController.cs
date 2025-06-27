using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "admin,superadmin")]
[ApiController]
[Route("api/v1/vendedoras/locales")]
public class RendimientoLocalesController : ControllerBase
{
    private readonly IRendimientoLocalesService _service;

    public RendimientoLocalesController(IRendimientoLocalesService service)
    {
        _service = service;
    }

    /// <summary>
    /// Obtener resumen de rendimiento de locales por sucursal, día y turno.
    /// </summary>
    [HttpGet("resumen")]
    public async Task<ActionResult<RendimientoLocalesResponseDto>> ObtenerResumenLocales(
        [FromQuery] RendimientoLocalesFilterDto filtros)
    {
        if (string.IsNullOrEmpty(filtros.SucursalNombre))
            return BadRequest(new { message = "El campo 'SucursalNombre' es obligatorio." });

        if (filtros.FechaInicio == default || filtros.FechaFin == default)
            return BadRequest(new { message = "Debe especificar un rango de fechas válido." });

        filtros.FechaInicio = AsegurarUtc(filtros.FechaInicio);
        filtros.FechaFin = AsegurarUtc(filtros.FechaFin);

        var resultado = await _service.ObtenerRendimientoLocalesAsync(filtros);
        return Ok(resultado);
    }

    private DateTime AsegurarUtc(DateTime fecha)
    {
        if (fecha.Kind == DateTimeKind.Unspecified)
            return DateTime.SpecifyKind(fecha, DateTimeKind.Utc);
        return fecha.ToUniversalTime();
    }
}
