using Microsoft.AspNetCore.Mvc;

namespace Notizap.Controllers
{
    [ApiController]
    [Route("api/mercadolibre")]
    public class MercadoLibreController : ControllerBase
    {
        private readonly IMercadoLibreService _service;

        public MercadoLibreController(IMercadoLibreService service)
        {
            _service = service;
        }

        [HttpGet("simple-stats")]
        public async Task<IActionResult> GetStats([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (from > to)
                return BadRequest(new { message = "'from' date cannot be greater than 'to' date." });

            var stats = await _service.GetStatsByRangeAsync(from, to);
            return Ok(stats);
        }

        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] int top = 5)
        {
            if (from > to)
                return BadRequest(new { message = "'from' date cannot be greater than 'to' date." });

            var products = await _service.GetTopProductsAsync(from, to, top);
            return Ok(products);
        }
    }
}
