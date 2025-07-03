using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/v1/instagram-analytics")]
[Authorize] // Requiere autenticación para todos los endpoints
public class InstagramAnalyticsController : ControllerBase
{
    private readonly IInstagramAnalyticsService _analyticsService;
    private readonly ILogger<InstagramAnalyticsController> _logger;

    public InstagramAnalyticsController(
        IInstagramAnalyticsService analyticsService,
        ILogger<InstagramAnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el dashboard principal con métricas agregadas
    /// </summary>
    /// <param name="cuenta">Nombre de la cuenta (montella, alenka, kids)</param>
    /// <param name="desde">Fecha inicio del período (opcional, default: hace 30 días)</param>
    /// <param name="hasta">Fecha fin del período (opcional, default: hoy)</param>
    [HttpGet("{cuenta}/dashboard")]
    [Authorize(Roles = "viewer,admin,superadmin")]
    public async Task<ActionResult<InstagramDashboardDto>> GetDashboard(
        [Required] string cuenta,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null)
    {
        try
        {
            if (!EsCuentaValida(cuenta))
            {
                return BadRequest($"Cuenta '{cuenta}' no válida. Cuentas disponibles: montella, alenka, kids");
            }

            var dashboard = await _analyticsService.GetDashboardAsync(cuenta, desde, hasta);
            
            _logger.LogInformation("Dashboard generado para cuenta {Cuenta} período {Desde} - {Hasta}", 
                cuenta, desde?.ToString("yyyy-MM-dd") ?? "30 días atrás", hasta?.ToString("yyyy-MM-dd") ?? "hoy");

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando dashboard para cuenta {Cuenta}", cuenta);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene análisis profundo de patrones temporales y de contenido
    /// </summary>
    [HttpGet("{cuenta}/analisis-patrones")]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<ActionResult<AnalisisPatronesDto>> GetAnalisisPatrones(
        [Required] string cuenta,
        [FromQuery, Required] DateTime desde,
        [FromQuery, Required] DateTime hasta)
    {
        try
        {
            if (!EsCuentaValida(cuenta))
            {
                return BadRequest($"Cuenta '{cuenta}' no válida");
            }

            if (desde >= hasta)
            {
                return BadRequest("La fecha 'desde' debe ser anterior a 'hasta'");
            }

            var diferenciaDias = (hasta - desde).TotalDays;
            if (diferenciaDias > 365)
            {
                return BadRequest("El período no puede ser mayor a 365 días");
            }

            var analisis = await _analyticsService.GetAnalisisPatronesAsync(cuenta, desde, hasta);
            
            _logger.LogInformation("Análisis de patrones generado para {Cuenta} período {Dias} días", 
                cuenta, diferenciaDias);

            return Ok(analisis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en análisis de patrones para cuenta {Cuenta}", cuenta);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene evolución detallada de seguidores con crecimiento diario
    /// </summary>
    [HttpGet("{cuenta}/evolucion-seguidores")]
    [Authorize(Roles = "viewer,admin,superadmin")]
    public async Task<ActionResult<List<EvolucionSeguidoresDto>>> GetEvolucionSeguidores(
        [Required] string cuenta,
        [FromQuery, Required] DateTime desde,
        [FromQuery, Required] DateTime hasta)
    {
        try
        {
            if (!EsCuentaValida(cuenta))
            {
                return BadRequest($"Cuenta '{cuenta}' no válida");
            }

            if (desde >= hasta)
            {
                return BadRequest("La fecha 'desde' debe ser anterior a 'hasta'");
            }

            var evolucion = await _analyticsService.GetEvolucionSeguidoresDetallada(cuenta, desde, hasta);
            
            return Ok(evolucion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo evolución de seguidores para {Cuenta}", cuenta);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene engagement rate promedio por tipo de contenido
    /// </summary>
    [HttpGet("{cuenta}/engagement-por-tipo")]
    [Authorize(Roles = "viewer,admin,superadmin")]
    public async Task<ActionResult<Dictionary<string, double>>> GetEngagementPorTipo(
        [Required] string cuenta,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null)
    {
        try
        {
            if (!EsCuentaValida(cuenta))
            {
                return BadRequest($"Cuenta '{cuenta}' no válida");
            }

            var fechaDesde = desde ?? DateTime.UtcNow.AddDays(-30);
            var fechaHasta = hasta ?? DateTime.UtcNow;

            var engagement = await _analyticsService.GetEngagementRatePorTipoAsync(cuenta, fechaDesde, fechaHasta);
            
            return Ok(engagement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo engagement por tipo para {Cuenta}", cuenta);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene los mejores horarios de publicación basado en engagement histórico
    /// </summary>
    [HttpGet("{cuenta}/mejores-horarios")]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<ActionResult<List<HorarioPerformanceDto>>> GetMejoresHorarios(
        [Required] string cuenta,
        [FromQuery] int diasAnalisis = 30)
    {
        try
        {
            if (!EsCuentaValida(cuenta))
            {
                return BadRequest($"Cuenta '{cuenta}' no válida");
            }

            if (diasAnalisis < 7 || diasAnalisis > 365)
            {
                return BadRequest("Los días de análisis deben estar entre 7 y 365");
            }

            var horarios = await _analyticsService.GetMejoresHorariosAsync(cuenta, diasAnalisis);
            
            return Ok(horarios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo mejores horarios para {Cuenta}", cuenta);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Genera resumen ejecutivo para reportes
    /// </summary>
    [HttpGet("{cuenta}/resumen-ejecutivo")]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<ActionResult<ResumenEjecutivoDto>> GetResumenEjecutivo(
        [Required] string cuenta,
        [FromQuery, Required] DateTime desde,
        [FromQuery, Required] DateTime hasta)
    {
        try
        {
            if (!EsCuentaValida(cuenta))
            {
                return BadRequest($"Cuenta '{cuenta}' no válida");
            }

            if (desde >= hasta)
            {
                return BadRequest("La fecha 'desde' debe ser anterior a 'hasta'");
            }

            var resumen = await _analyticsService.GetResumenEjecutivoAsync(cuenta, desde, hasta);
            
            _logger.LogInformation("Resumen ejecutivo generado para {Cuenta}", cuenta);

            return Ok(resumen);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando resumen ejecutivo para {Cuenta}", cuenta);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Compara métricas entre dos períodos
    /// </summary>
    [HttpGet("{cuenta}/comparar-periodos")]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<ActionResult<ComparativaPeriodosDto>> CompararPeriodos(
        [Required] string cuenta,
        [FromQuery, Required] DateTime periodoActualDesde,
        [FromQuery, Required] DateTime periodoActualHasta,
        [FromQuery, Required] DateTime periodoAnteriorDesde,
        [FromQuery, Required] DateTime periodoAnteriorHasta)
    {
        try
        {
            if (!EsCuentaValida(cuenta))
            {
                return BadRequest($"Cuenta '{cuenta}' no válida");
            }

            // Validaciones de fechas
            if (periodoActualDesde >= periodoActualHasta)
            {
                return BadRequest("Las fechas del período actual no son válidas");
            }

            if (periodoAnteriorDesde >= periodoAnteriorHasta)
            {
                return BadRequest("Las fechas del período anterior no son válidas");
            }

            var diasActual = (periodoActualHasta - periodoActualDesde).TotalDays;
            var diasAnterior = (periodoAnteriorHasta - periodoAnteriorDesde).TotalDays;

            if (Math.Abs(diasActual - diasAnterior) > 7)
            {
                return BadRequest("Los períodos deben tener duraciones similares (diferencia máxima: 7 días)");
            }

            var comparativa = await _analyticsService.GetComparativaPeriodosAsync(
                cuenta, periodoActualDesde, periodoActualHasta, 
                periodoAnteriorDesde, periodoAnteriorHasta);
            
            _logger.LogInformation("Comparativa de períodos generada para {Cuenta}", cuenta);

            return Ok(comparativa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en comparativa de períodos para {Cuenta}", cuenta);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Endpoint de salud para verificar que el servicio está funcionando
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            service = "Instagram Analytics API"
        });
    }

    // Métodos auxiliares privados
    private static bool EsCuentaValida(string cuenta)
    {
        var cuentasValidas = new[] { "montella", "alenka", "kids" };
        return cuentasValidas.Contains(cuenta.ToLower());
    }
}