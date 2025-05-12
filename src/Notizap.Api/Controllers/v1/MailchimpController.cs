using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NotiZap.Dashboard.API.Controllers;

[Authorize(Roles = "viewer,admin,superadmin")]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class MailchimpController : ControllerBase
{
  private readonly IMailchimpService _service;

  public MailchimpController(IMailchimpService service)
  {
    _service = service;
  }

  [HttpGet("stats")]
  public async Task<IActionResult> GetStats([FromQuery] string? campaignId)
  {
    var result = await _service.GetCampaignStatsAsync(campaignId);
    return Ok(result);
  }

  [HttpGet("campaigns")]
  public async Task<IActionResult> GetCampaigns()
  {
    var list = await _service.GetAvailableCampaignsAsync();
    return Ok(list);
  }

  [HttpGet("highlights")]
  public async Task<IActionResult> GetHighlights()
  {
    var result = await _service.GetHighlightsAsync();
    return Ok(result);
  }
}
