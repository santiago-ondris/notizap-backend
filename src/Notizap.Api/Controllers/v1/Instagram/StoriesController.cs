using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Notizap.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/instagram")]
[Authorize(Roles = "viewer,admin,superadmin")]
public class StoriesController : ControllerBase
{
    private readonly IStoriesService _storiesService;

    public StoriesController(IStoriesService storiesService)
    {
        _storiesService = storiesService;
    }

    [HttpGet("{account}/stories/top")]
    [SwaggerOperation(Summary = "Obtener Stories ordenadas")]
    public async Task<ActionResult<List<InstagramStory>>> GetTopStories(
        string account,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery(Name = "ordenarPor")] string ordenarPor = "impressions",
        [FromQuery] int limit = 50)
    {
        try
        {
            var result = await _storiesService.GetTopStoriesAsync(account, from, to, ordenarPor, limit);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Error al obtener las historias",
                exception = ex.Message
            });
        }
    }

    [HttpPost("{account}/stories/sync")]
    [SwaggerOperation(Summary = "Obtener Stories desde la API de Metricool")]
    public async Task<IActionResult> SyncInstagramStories(string account, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        try
        {
            var nuevos = await _storiesService.SyncInstagramStoriesAsync(account, from, to);
            return Ok(new { message = $"{nuevos} historias nuevas sincronizadas." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "Error al sincronizar historias.",
                exception = ex.Message
            });
        }
    }
}    