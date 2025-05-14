using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    /// <summary>
    /// Devuelve campañas de Mailchimp almacenadas en la base de datos
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string cuenta)
    {
        var data = await _queryService.GetAllCampaignsAsync(cuenta);
        return Ok(data);
    }

    /// <summary>
    /// Devuelve estadísticas de una campaña por ID (desde DB)
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats([FromQuery] string campaignId)
    {
        var result = await _queryService.GetStatsByCampaignIdAsync(campaignId);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Devuelve las campañas destacadas (mayor open, click y conversion)
    /// </summary>
    [HttpGet("highlights")]
    public async Task<IActionResult> GetHighlights([FromQuery] string cuenta)
    {
        var result = await _queryService.GetHighlightsAsync(cuenta);
        return Ok(result);
    }

    /// <summary>
    /// Sincroniza campañas desde la API de Mailchimp (solo admin)
    /// </summary>
    [HttpPost("sync")]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<IActionResult> Sync([FromQuery] string cuenta)
    {
        var nuevas = await _syncService.SyncAsync(cuenta);

        if (nuevas == 0)
            return Ok("No se añadieron campañas nuevas, todas se encontraban ya en la base de datos.");
        else
            return Ok($"{nuevas} campaña(s) nueva(s) añadida(s) a la base de datos.");
    }
}
