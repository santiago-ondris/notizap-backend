using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace NotiZap.Dashboard.API.Controllers;

[Authorize(Roles = "viewer,admin,superadmin")]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class MailchimpController : ControllerBase
{
    private readonly IMailchimpQueryService _queryService;
    private readonly IMailchimpSyncService _syncService;
    private readonly ILogger<MailchimpController> _logger;

    public MailchimpController(
        IMailchimpQueryService queryService,
        IMailchimpSyncService syncService,
        ILogger<MailchimpController> logger)
    {
        _queryService = queryService;
        _syncService = syncService;
        _logger = logger;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Obtener campañas de Mailchimp de la DB")]
    public async Task<IActionResult> GetAll([FromQuery] string cuenta)
    {
        var user = User.Identity?.Name ?? "anonymous";
        _logger.LogInformation("Usuario {User} solicita campañas de Mailchimp para cuenta {Cuenta}", user, cuenta);

        try
        {
            var data = await _queryService.GetAllCampaignsAsync(cuenta);
            _logger.LogInformation("Campañas de Mailchimp obtenidas correctamente para cuenta {Cuenta}. Cantidad: {Cantidad}", cuenta, data.Count);
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener campañas de Mailchimp para cuenta {Cuenta} por usuario {User}", cuenta, user);
            return StatusCode(500, "Error al obtener campañas de Mailchimp");
        }
    }

    [HttpGet("stats")]
    [SwaggerOperation(Summary = "Obtener una campaña de Mailchimp por Id")]
    public async Task<IActionResult> GetStats([FromQuery] string campaignId)
    {
        var user = User.Identity?.Name ?? "anonymous";
        _logger.LogInformation("Usuario {User} solicita estadísticas de campaña {CampaignId}", user, campaignId);

        try
        {
            var result = await _queryService.GetStatsByCampaignIdAsync(campaignId);
            
            if (result is null)
            {
                _logger.LogWarning("No se encontraron estadísticas para campaña {CampaignId} solicitada por usuario {User}", campaignId, user);
                return NotFound();
            }

            _logger.LogInformation("Estadísticas de campaña {CampaignId} obtenidas correctamente por usuario {User}", campaignId, user);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de campaña {CampaignId} por usuario {User}", campaignId, user);
            return StatusCode(500, "Error al obtener estadísticas de la campaña");
        }
    }

    [HttpGet("highlights")]
    [SwaggerOperation(Summary = "Obtener campañas destacadas (mayor open, click y conversion)")]
    public async Task<IActionResult> GetHighlights([FromQuery] string cuenta)
    {
        var user = User.Identity?.Name ?? "anonymous";
        _logger.LogInformation("Usuario {User} solicita highlights de campañas para cuenta {Cuenta}", user, cuenta);

        try
        {
            var result = await _queryService.GetHighlightsAsync(cuenta);
            _logger.LogInformation("Highlights de campañas obtenidos correctamente para cuenta {Cuenta} por usuario {User}", cuenta, user);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener highlights para cuenta {Cuenta} por usuario {User}", cuenta, user);
            return StatusCode(500, "Error al obtener highlights de campañas");
        }
    }

    [HttpPost("sync")]
    [SwaggerOperation(Summary = "Sincroniza campañas desde la API de Mailchimp")]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<IActionResult> Sync([FromQuery] string cuenta)
    {
        var user = User.Identity?.Name ?? "anonymous";
        _logger.LogInformation("Usuario {User} inicia sincronización de campañas Mailchimp para cuenta {Cuenta}", user, cuenta);

        try
        {
            var resultado = await _syncService.SyncAsync(cuenta);
            
            _logger.LogInformation(
                "Sincronización de Mailchimp completada para cuenta {Cuenta} por usuario {User}. Nuevas: {Nuevas}, Actualizadas: {Actualizadas}, Total: {Total}",
                cuenta, user, resultado.NuevasCampañas, resultado.CampañasActualizadas, resultado.TotalProcesadas);

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico durante sincronización de Mailchimp para cuenta {Cuenta} por usuario {User}", cuenta, user);
            return StatusCode(500, "Error durante la sincronización con Mailchimp");
        }
    }

    [HttpPatch("{campaignId}/title")]
    [SwaggerOperation(Summary = "Actualiza el título de una campaña")]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<IActionResult> UpdateTitle(
        [FromRoute] int campaignId, 
        [FromBody] UpdateCampaignTitleDto request)
    {
        var user = User.Identity?.Name ?? "anonymous";
        _logger.LogInformation("Usuario {User} solicita actualizar título de campaña Id {CampaignId} a '{NewTitle}'", user, campaignId, request.Title);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Modelo inválido para actualización de título de campaña {CampaignId} por usuario {User}", campaignId, user);
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _queryService.UpdateCampaignTitleAsync(campaignId, request.Title);

            if (!updated)
            {
                _logger.LogWarning("No se encontró campaña con Id {CampaignId} para actualizar título por usuario {User}", campaignId, user);
                return NotFound($"No se encontró la campaña con ID {campaignId}");
            }

            _logger.LogInformation("Título de campaña Id {CampaignId} actualizado correctamente a '{NewTitle}' por usuario {User}", campaignId, request.Title, user);
            return Ok(new { message = "Título actualizado exitosamente", title = request.Title });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar título de campaña Id {CampaignId} por usuario {User}", campaignId, user);
            return StatusCode(500, "Error al actualizar el título de la campaña");
        }
    }
}