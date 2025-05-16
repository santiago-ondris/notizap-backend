using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Notizap.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/instagram")]
[Authorize(Roles = "viewer,admin,superadmin")]
public class PostsController : ControllerBase
{
    private readonly IPostsService _postsService;

    public PostsController(IPostsService postsService)
    {
        _postsService = postsService;
    }

    [HttpGet("{account}/posts/top")]
    [SwaggerOperation(Summary = "Obtener posteos de Instagram")]
    public async Task<ActionResult<List<InstagramPost>>> GetTopPosts(
        string account,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery(Name = "ordenarPor")] string ordenarPor = "likes")
    {
        try
        {
            var result = await _postsService.GetTopPostsAsync(account, from, to, ordenarPor);
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
                message = "Error al obtener los posteos.",
                exception = ex.Message
            });
        }
    }
    [HttpPost("{account}/posts/sync")]
    [SwaggerOperation(Summary = "Obtener posteos de Instagram desde la API de Metricool")]
    public async Task<IActionResult> SyncInstagramPosts(string account, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        try
        {
            var nuevos = await _postsService.SyncInstagramPostsAsync(account, from, to);
            return Ok(new { message = $"{nuevos} posteos nuevos sincronizados." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "Error al sincronizar posteos.",
                exception = ex.Message
            });
        }
    }
}    