using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Notizap.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/instagram")]
[Authorize(Roles = "viewer,admin,superadmin")]
public class FollowersController : ControllerBase
{
    private readonly IFollowersService _followersService;

    public FollowersController(IFollowersService followersService)
    {
        _followersService = followersService;
    }

    [HttpGet("{account}/followers")]
    [SwaggerOperation(Summary = "Obtener seguidores de Instagram")]
    public async Task<ActionResult<List<FollowerDayData>>> GetFollowersMetrics(string account, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        try
        {
            var result = await _followersService.GetFollowersMetricsAsync(account, from, to);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Success = false,
                Message = "Error interno del servidor.",
                Exception = ex.Message
            });
        }
    }
}    