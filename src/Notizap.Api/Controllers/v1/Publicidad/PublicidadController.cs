using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public PublicidadController(
        IAdService adService,
        IMixedAdsService mixedAdsService)
    {
        _adService       = adService;
        _mixedAdsService = mixedAdsService;
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
}
