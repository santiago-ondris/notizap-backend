using Microsoft.AspNetCore.Mvc;

namespace NotiZap.Dashboard.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WooCommerceController : ControllerBase
    {
        private readonly IWooCommerceService _wooService;

        public WooCommerceController(IWooCommerceService wooService)
        {
            _wooService = wooService;
        }

        // Simple-stats por rango
        [HttpGet("simple-stats")]
        public async Task<ActionResult<SalesStatsDto>> GetSimpleStats(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            if (from > to)
                return BadRequest(new { message = "'from' no puede ser posterior a 'to'." });

            var stats = await _wooService.GetStatsByRangeAsync(from, to);
            return Ok(stats);
        }

        // Nuevo: Top productos
        [HttpGet("top-products")]
        public async Task<ActionResult<List<ProductStatsDto>>> GetTopProducts(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] int top = 5)
        {
            if (from > to)
                return BadRequest(new { message = "'from' no puede ser posterior a 'to'." });

            var list = await _wooService.GetTopProductsAsync(from, to, top);
            return Ok(list);
        }
    }
}
