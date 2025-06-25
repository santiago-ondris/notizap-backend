using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notizap.Application.Ads.Dtos;
using Notizap.Application.Ads.Services;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Route("api/v{version:apiVersion}/publicidad")]
[ApiVersion("1.0")]
[Authorize]
public class PublicidadController : ControllerBase
{
    private readonly IAdService _adService;
    private readonly IMixedAdsService _mixedAdsService;
    private readonly NotizapDbContext _context;
    private readonly IUpdateAdService _updateAdService;

    public PublicidadController(
        IAdService adService,
        NotizapDbContext context,
        IMixedAdsService mixedAdsService,
        IUpdateAdService updateAdService)
    {
        _adService       = adService;
        _mixedAdsService = mixedAdsService;
        _context         = context;
        _updateAdService = updateAdService;
    }

    [HttpGet]
    [Authorize(Roles = "viewer,admin,superadmin")]
    [SwaggerOperation(Summary = "Obtener todos los reportes de campañas")]
    public async Task<ActionResult<List<AdReportDto>>> GetAll()
    {
        var result = await _adService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "viewer,admin,superadmin")]
    [SwaggerOperation(Summary = "Obtener un reporte de campaña por Id")]
    public async Task<ActionResult<AdReportDto>> GetById(int id)
    {
        var result = await _adService.GetByIdAsync(id);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "admin,superadmin")]
    [SwaggerOperation(Summary = "Crear un reporte de campañas")]
    public async Task<ActionResult<AdReportDto>> Create(SaveAdReportDto dto)
    {
        var result = await _adService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,superadmin")]
    [SwaggerOperation(Summary = "Actualizar un reporte de campañas")]
    public async Task<ActionResult<AdReportDto>> Update(int id, SaveAdReportDto dto)
    {
        var result = await _adService.UpdateAsync(id, dto);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,superadmin")]
    [SwaggerOperation(Summary = "Eliminar un reporte de campañas")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _adService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpGet("stats/resumen")]
    [Authorize(Roles = "viewer,admin,superadmin")]
    [SwaggerOperation(Summary = "Obtener resumen mensual de campañas")]
    public async Task<ActionResult<List<PublicidadResumenMensualDto>>> GetResumenMensual(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] string? unidad = null)
    {
        if (month < 1 || month > 12)
            return BadRequest("Mes inválido. Debe estar entre 1 y 12.");

        var result = await _adService.GetResumenMensualAsync(year, month, unidad!);
        return Ok(result);
    }

    [HttpGet("meta-insights")]
    [Authorize(Roles = "admin,superadmin")]
    [SwaggerOperation(Summary = "Obtener metricas campañas desde Meta")]
    public async Task<ActionResult<List<MetaCampaignInsightDto>>> GetMetaInsights(
        [FromQuery] string unidad,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromServices] IMetaAdsService metaAdsService)
    {
        var accountId = unidad.ToLower() switch
        {
            "montella" => "act_70862159",
            "alenka"   => "act_891129171656093",
            _ => throw new ArgumentException("Unidad no reconocida.")
        };

        var result = await metaAdsService.GetCampaignInsightsAsync(accountId, from, to);
        return Ok(result);
    }

    [HttpGet("mixed-insights")]
    [Authorize(Roles = "admin,superadmin")]
    [SwaggerOperation(Summary = "Obtener un reporte con metricas de la API y manuales")]
    public async Task<ActionResult<List<MixedCampaignInsightDto>>> GetMixedInsights(
        [FromQuery] string unidad,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int reportId)
    {
        var accountId = unidad.ToLower() switch
        {
            "montella" => "act_70862159",
            "alenka"   => "act_891129171656093",
            _ => throw new ArgumentException("Unidad no reconocida.")
        };

        var insights = await _mixedAdsService
            .GetMixedCampaignInsightsAsync(accountId, from, to, reportId);

        return Ok(insights);
    }

    [HttpPost("sync")]
    [Authorize(Roles = "admin,superadmin")]
    [SwaggerOperation(Summary = "Sincronizar metricas de la API de meta a la DB")]
    public async Task<ActionResult<SyncResultDto>> SyncFromApi(
        [FromQuery] string unidad,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var result = await _adService
            .SyncReportFromApiAsync(unidad, from, to);
        return Ok(result);
    }

    [HttpGet("dashboard")]
    [Authorize(Roles = "viewer,admin,superadmin")]
    [SwaggerOperation(Summary = "Obtener dashboard ejecutivo de publicidad")]
    public async Task<ActionResult<PublicidadDashboardDto>> GetDashboard(
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null,
        [FromQuery] int? mesesHaciaAtras = 6,
        [FromQuery] string[]? unidadesNegocio = null,
        [FromQuery] string[]? plataformas = null,
        [FromQuery] int topCampañasLimit = 10)
    {
        try
        {
            var parametros = new PublicidadDashboardParamsDto
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                MesesHaciaAtras = mesesHaciaAtras,
                UnidadesNegocio = unidadesNegocio?.ToList() ?? new List<string>(),
                Plataformas = plataformas?.ToList() ?? new List<string>(),
                TopCampañasLimit = Math.Min(topCampañasLimit, 50) // Máximo 50 campañas
            };

            var dashboard = await _adService.GetDashboardDataAsync(parametros);
            return Ok(dashboard);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
    {
            // Log the exception
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("dashboard/filters")]
    [Authorize(Roles = "viewer,admin,superadmin")]
    [SwaggerOperation(Summary = "Obtener opciones disponibles para filtros del dashboard")]
    public async Task<ActionResult<PublicidadDashboardFiltersDto>> GetDashboardFilters()
    {
        try
        {
            // Obtener todas las unidades y plataformas disponibles en la BD
            var unidadesDisponibles = await _context.AdReports
                .Select(r => r.UnidadNegocio)
                .Distinct()
                .OrderBy(u => u)
                .ToListAsync();

            var plataformasDisponibles = await _context.AdReports
                .Select(r => r.Plataforma)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();

            // Obtener rango de fechas disponible
            var fechaMinima = await _context.AdReports
                .OrderBy(r => r.Year)
                .ThenBy(r => r.Month)
                .Select(r => new DateTime(r.Year, r.Month, 1))
                .FirstOrDefaultAsync();

            var fechaMaxima = await _context.AdReports
                .OrderByDescending(r => r.Year)
                .ThenByDescending(r => r.Month)
                .Select(r => new DateTime(r.Year, r.Month, 1))
                .FirstOrDefaultAsync();

            var filtros = new PublicidadDashboardFiltersDto
            {
                UnidadesNegocio = unidadesDisponibles,
                Plataformas = plataformasDisponibles,
                FechaMinima = fechaMinima,
                FechaMaxima = fechaMaxima
            };

            return Ok(filtros);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Error obteniendo filtros disponibles" });
        }
    }

    [HttpGet("campanas")]
    [Authorize(Roles = "viewer,admin,superadmin")]
    [SwaggerOperation(Summary = "Obtener todas las campañas con filtros y paginación")]
    public async Task<ActionResult<object>> GetAllCampaigns(
        [FromQuery] string? unidad = null,
        [FromQuery] string? plataforma = null,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            // Validar parámetros de paginación
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var campaigns = await _updateAdService.GetAllCampaignsAsync(
                unidad, plataforma, year, month, page, pageSize);

            var totalCount = await _updateAdService.GetTotalCampaignsCountAsync(
                unidad, plataforma, year, month);

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var result = new
            {
                data = campaigns,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = totalPages,
                    hasNext = page < totalPages,
                    hasPrevious = page > 1
                },
                filters = new
                {
                    unidad = unidad,
                    plataforma = plataforma,
                    year = year,
                    month = month
                }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener las campañas", error = ex.Message });
        }
    }

    [HttpGet("campanas/{id}")]
    [Authorize(Roles = "viewer,admin,superadmin")]
    [SwaggerOperation(Summary = "Obtener una campaña específica por ID")]
    public async Task<ActionResult<AdCampaignReadDto>> GetCampaignById(int id)
    {
        try
        {
            var campaign = await _updateAdService.GetCampaignByIdAsync(id);
            
            if (campaign == null)
                return NotFound(new { message = $"No se encontró la campaña con ID {id}" });

            return Ok(campaign);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener la campaña", error = ex.Message });
        }
    }

    [HttpPut("campanas/{id}")]
    [Authorize(Roles = "admin,superadmin")]
    [SwaggerOperation(Summary = "Actualizar una campaña específica")]
    public async Task<ActionResult<AdCampaignReadDto>> UpdateCampaign(int id, UpdateAdCampaignDto dto)
    {
        try
        {
            if (id <= 0)
                return BadRequest(new { message = "ID de campaña inválido" });

            var updatedCampaign = await _updateAdService.UpdateCampaignAsync(id, dto);
            
            if (updatedCampaign == null)
                return NotFound(new { message = $"No se encontró la campaña con ID {id}" });

            return Ok(updatedCampaign);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar la campaña", error = ex.Message });
        }
    }

    [HttpGet("campanas/stats")]
    [Authorize(Roles = "viewer,admin,superadmin")]
    [SwaggerOperation(Summary = "Obtener estadísticas generales de campañas")]
    public async Task<ActionResult<object>> GetCampaignsStats(
        [FromQuery] string? unidad = null,
        [FromQuery] string? plataforma = null,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null)
    {
        try
        {
            var totalCampaigns = await _updateAdService.GetTotalCampaignsCountAsync(
                unidad, plataforma, year, month);

            // Obtener una muestra para estadísticas rápidas
            var sampleCampaigns = await _updateAdService.GetAllCampaignsAsync(
                unidad, plataforma, year, month, 1, totalCampaigns);

            var stats = new
            {
                totalCampaigns = totalCampaigns,
                totalInvestment = sampleCampaigns.Sum(c => c.MontoInvertido),
                avgInvestmentPerCampaign = totalCampaigns > 0 ? sampleCampaigns.Average(c => c.MontoInvertido) : 0,
                campaignsWithMetrics = sampleCampaigns.Count(c => c.Clicks > 0 || c.Impressions > 0),
                manualCampaigns = sampleCampaigns.Count(c => c.Clicks == 0 && c.Impressions == 0),
                unidadesRepresented = sampleCampaigns.GroupBy(c => c.UnidadNegocio).Count(),
                plataformasRepresented = sampleCampaigns.GroupBy(c => c.Plataforma).Count()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener estadísticas", error = ex.Message });
        }
    }
}
