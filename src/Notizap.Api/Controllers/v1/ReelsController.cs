using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ReelsController(IReelsService service) : ControllerBase
{
    private readonly IReelsService _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var reel = await _service.GetByIdAsync(id);
        return reel is null ? NotFound() : Ok(reel);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReelDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ReelDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }
    [HttpGet("top-views")]
    public async Task<IActionResult> GetTopByViews([FromQuery] int count = 5)
    {
        var top = await _service.GetTopByViewsAsync(count);
        return Ok(top);
    }

    [HttpGet("top-likes")]
    public async Task<IActionResult> GetTopByLikes([FromQuery] int count = 5)
    {
        var top = await _service.GetTopByLikesAsync(count);
        return Ok(top);
    }
    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImage(IFormFile file, [FromServices] IImageUploadService imageService)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var url = await imageService.UploadImageAsync(file);
        return Ok(new { url });
    }
}
