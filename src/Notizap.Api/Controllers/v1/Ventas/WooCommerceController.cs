using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace NotiZap.Dashboard.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class WooCommerceController : ControllerBase
{
    private readonly IWooCommerceService _wooService;
    private readonly NotizapDbContext _context;

    public WooCommerceController(IWooCommerceService wooService, NotizapDbContext context)
    {
        _wooService = wooService;
        _context = context;
    }

    [Authorize(Roles = "admin,superadmin")]
    [HttpGet("simple-stats")]
    [SwaggerOperation(Summary = "Llamar a la API de WooCommere para obtener metricas de ventas")]
    public async Task<ActionResult<SalesStatsDto>> GetSimpleStats(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] WooCommerceStore store)
    {
        if (from > to)
            return BadRequest(new { message = "'from' no puede ser posterior a 'to'." });

        var stats = await _wooService.GetStatsByRangeAsync(from, to, store);
        return Ok(stats);
    }

    [Authorize(Roles = "admin,superadmin")]
    [HttpGet("top-products")]
    [SwaggerOperation(Summary = "Llamar a la API de WooCommere para obtener top productos vendidos")]
    public async Task<ActionResult<List<ProductStatsDto>>> GetTopProducts(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] WooCommerceStore store,
        [FromQuery] int top = 5)
    {
        if (from > to)
            return BadRequest(new { message = "'from' no puede ser posterior a 'to'." });

        var list = await _wooService.GetTopProductsAsync(from, to, store, top);
        return Ok(list);
    }

    [Authorize(Roles = "admin,superadmin")]
    [HttpGet("monthly")]
    [SwaggerOperation(Summary = "Llamar a la API de WooCommere para obtener resumen vensual de ventas")]
    public async Task<ActionResult<SalesStatsDto>> GetMonthly(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] WooCommerceStore store)
    {
        if (month < 1 || month > 12)
            return BadRequest(new { message = "Mes inválido (debe estar entre 1 y 12)." });

        var stats = await _wooService.GetMonthlyStatsSimpleAsync(year, month, store);
        return Ok(stats);
    }

    [Authorize(Roles = "admin,superadmin")]
    [HttpPost("monthly-report/save")]
    [SwaggerOperation(Summary = "Guardar reporte de ventas en la DB")]
    public async Task<ActionResult<WooCommerceMonthlyReport>> SaveMonthly([FromBody] SaveWooMonthlyReportDto dto)
    {
        var result = await _wooService.SaveMonthlyReportAsync(dto);
        return Ok(result);
    }

    [Authorize(Roles = "viewer,admin,superadmin")]
    [HttpGet("monthly-report")]
    [SwaggerOperation(Summary = "Obtener reporte mensual de ventas de la DB")]
    public async Task<ActionResult<WooCommerceMonthlyReport>> GetSavedMonthlyReport(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] WooCommerceStore store)
    {
        var report = await _wooService.GetSavedMonthlyReportAsync(year, month, store);
        if (report == null)
            return NotFound(new { message = "No hay datos guardados para ese mes y tienda." });

        return Ok(report);
    }

    [Authorize(Roles = "admin,superadmin")]
    [HttpPut("monthly-report/{id}")]
    [SwaggerOperation(Summary = "Actualizar reporte en la DB")]
    public async Task<IActionResult> UpdateMonthlyReport(int id, [FromBody] SaveWooMonthlyReportDto dto)
    {
        var report = await _context.WooCommerceMonthlyReports
            .Include(r => r.DailySales)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report == null)
            return NotFound(new { message = "Reporte no encontrado" });

        report.Year = dto.Year;
        report.Month = dto.Month;
        report.Store = dto.Store;
        report.UnitsSold = dto.UnitsSold;
        report.Revenue = dto.Revenue;

        _context.WooDailySales.RemoveRange(report.DailySales);
        report.DailySales = dto.DailySales.Select(d => new WooDailySale
        {
            Date = d.Date,
            UnitsSold = d.UnitsSold,
            Revenue = d.Revenue
        }).ToList();

        await _context.SaveChangesAsync();
        return Ok(report);
    }

    [Authorize(Roles = "admin,superadmin")]
    [HttpDelete("monthly-report/{id}")]
    [SwaggerOperation(Summary = "Eliminar reporte de la DB")]
    public async Task<IActionResult> DeleteMonthlyReport(int id)
    {
        var report = await _context.WooCommerceMonthlyReports
            .Include(r => r.DailySales)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report == null)
            return NotFound();

        _context.WooDailySales.RemoveRange(report.DailySales);
        _context.WooCommerceMonthlyReports.Remove(report);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Roles = "viewer,admin,superadmin")]
    [HttpGet("reports")]
    [SwaggerOperation(Summary = "Obtener todos los reportes")]
    public async Task<ActionResult<List<WooCommerceMonthlyReport>>> GetAllReports([FromQuery] WooCommerceStore store)
    {
        var list = await _context.WooCommerceMonthlyReports
            .Include(r => r.DailySales)
            .Where(r => r.Store == store)
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Month)
            .ToListAsync();

        return Ok(list);
    }
}
