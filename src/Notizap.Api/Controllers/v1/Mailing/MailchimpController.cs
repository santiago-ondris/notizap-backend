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

    public MailchimpController(
        IMailchimpQueryService queryService,
        IMailchimpSyncService syncService)
    {
        _queryService = queryService;
        _syncService = syncService;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Obtener campañas de Mailchimp de la DB")]
    public async Task<IActionResult> GetAll([FromQuery] string cuenta)
    {
        var data = await _queryService.GetAllCampaignsAsync(cuenta);
        return Ok(data);
    }

    [HttpGet("stats")]
    [SwaggerOperation(Summary = "Obtener una campaña de Mailchimp por Id")]
    public async Task<IActionResult> GetStats([FromQuery] string campaignId)
    {
        var result = await _queryService.GetStatsByCampaignIdAsync(campaignId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("highlights")]
    [SwaggerOperation(Summary = "Obtener campañas destacadas (mayor open, click y conversion)")]
    public async Task<IActionResult> GetHighlights([FromQuery] string cuenta)
    {
        var result = await _queryService.GetHighlightsAsync(cuenta);
        return Ok(result);
    }

    [HttpPost("sync")]
    [SwaggerOperation(Summary = "Sincroniza campañas desde la API de Mailchimp")]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<IActionResult> Sync([FromQuery] string cuenta)
    {
        var resultado = await _syncService.SyncAsync(cuenta);
        return Ok(resultado);
    }

    [HttpPatch("{campaignId}/title")]
    [SwaggerOperation(Summary = "Actualiza el título de una campaña")]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<IActionResult> UpdateTitle(
        [FromRoute] int campaignId, 
        [FromBody] UpdateCampaignTitleDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _queryService.UpdateCampaignTitleAsync(campaignId, request.Title);

        if (!updated)
            return NotFound($"No se encontró la campaña con ID {campaignId}");

        return Ok(new { message = "Título actualizado exitosamente", title = request.Title });
    }
}