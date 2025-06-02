using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Notizap.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/instagram")]
[Authorize(Roles = "viewer,admin,superadmin")]
public class ReelsController : ControllerBase
{
    private readonly IReelsService _reelsService;

    public ReelsController(IReelsService reelsService)
    {
        _reelsService = reelsService;
    }

    [HttpPost("{account}/reels/sync")]
    [SwaggerOperation(Summary = "Obtener Reels desde la API de Metricool")]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<IActionResult> SyncInstagramReels(string account, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        try
        {
            var nuevos = await _reelsService.SyncInstagramReelsAsync(account, from, to);
            return Ok(new { message = $"{nuevos} reels nuevos guardados." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "Error al sincronizar reels.",
                exception = ex.Message
            });
        }
    }

    [HttpGet("{account}/reels/top-views")]
    [SwaggerOperation(Summary = "Obtener Reels con mas vistas")]
    public async Task<ActionResult<List<InstagramReel>>> GetTopReelsByViews(string account, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await _reelsService.GetTopReelsByViewsAsync(account, from, to);
        return Ok(result);
    }

    [HttpGet("{account}/reels/top-likes")]
    [SwaggerOperation(Summary = "Obtener Reels con mas likes")]
    public async Task<ActionResult<List<InstagramReel>>> GetTopReelsByLikes(string account, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await _reelsService.GetTopReelsByLikesAsync(account, from, to);
        return Ok(result);
    }

    [HttpGet("{account}/reels/all")]
    [SwaggerOperation(Summary = "Obtener todos los Reels")]
    public async Task<ActionResult<List<InstagramReel>>> GetAllReels(string account, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await _reelsService.GetAllReelsAsync(account, from, to);
        return Ok(result);
    }
}
