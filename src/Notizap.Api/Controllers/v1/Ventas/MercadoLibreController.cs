using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class MercadoLibreController : ControllerBase
{
    private readonly IMercadoLibreService _service;
    private readonly IMercadoLibrePublicidadService _adsService;
    private readonly IMercadoLibreExcelProcessor _excelProcessor;

    public MercadoLibreController(IMercadoLibreService service, IMercadoLibrePublicidadService adsService, IMercadoLibreExcelProcessor excelProcessor)
    {
        _service = service;
        _adsService = adsService;
        _excelProcessor = excelProcessor;
    }

    [Authorize(Roles = "viewer,admin,superadmin")]
    [HttpGet]
    [SwaggerOperation(Summary = "Obtener todos los reportes de ML")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [Authorize(Roles = "viewer,admin,superadmin")]
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obtener reporte de ML por Id")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [Authorize(Roles = "admin,superadmin")]
    [HttpPost]
    [SwaggerOperation(Summary = "Crear reporte")]
    public async Task<IActionResult> Create([FromBody] MercadoLibreManualDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "admin,superadmin")]
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Actualizar reporte")]
    public async Task<IActionResult> Update(int id, [FromBody] MercadoLibreManualDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [Authorize(Roles = "admin,superadmin")]
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Eliminar reporte")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    #region Publicidad MercadoLibre

    [Authorize(Roles = "admin,superadmin")]
    [HttpPost("ads/product")]
    [SwaggerOperation(Summary = "Crear reporte de Product Ads")]
    public async Task<IActionResult> CrearProductAd([FromBody] CreateProductAdDto dto)
    {
        var id = await _adsService.CrearProductAdAsync(dto);
        return CreatedAtAction(nameof(GetAdById), new { id }, null);
    }

    [Authorize(Roles = "admin,superadmin")]
    [HttpPost("ads/brand")]
    [SwaggerOperation(Summary = "Crear reporte de Brand Ads")]
    public async Task<IActionResult> CrearBrandAd([FromBody] CreateBrandAdDto dto)
    {
        var id = await _adsService.CrearBrandAdAsync(dto);
        return CreatedAtAction(nameof(GetAdById), new { id }, null);
    }

    [Authorize(Roles = "admin,superadmin")]
    [HttpPost("ads/display")]
    [SwaggerOperation(Summary = "Crear reporte de Display Ads")]
    public async Task<IActionResult> CrearDisplayAd([FromBody] CreateDisplayAdDto dto)
    {
        var id = await _adsService.CrearDisplayAdAsync(dto);
        return CreatedAtAction(nameof(GetAdById), new { id }, null);
    }

    [Authorize(Roles = "viewer,admin,superadmin")]
    [HttpGet("ads")]
    [SwaggerOperation(Summary = "Listar todos los reportes publicitarios")]
    public async Task<IActionResult> GetAllAds()
    {
        var result = await _adsService.ObtenerTodosAsync();
        return Ok(result);
    }

    [Authorize(Roles = "viewer,admin,superadmin")]
    [HttpGet("ads/{id}")]
    [SwaggerOperation(Summary = "Obtener reporte publicitario por ID")]
    public async Task<IActionResult> GetAdById(int id)
    {
        var result = await _adsService.ObtenerPorIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [Authorize(Roles = "viewer,admin,superadmin")]
    [HttpGet("ads/mes")]
    [SwaggerOperation(Summary = "Obtener reportes por mes y año")]
    public async Task<IActionResult> GetAdsByMes([FromQuery] int year, [FromQuery] int month)
    {
        var result = await _adsService.ObtenerPorMesAsync(year, month);
        return Ok(result);
    }

    [Authorize(Roles = "viewer,admin,superadmin")]
    [HttpGet("ads/resumen")]
    [SwaggerOperation(Summary = "Obtener inversión total por mes")]
    public async Task<IActionResult> GetResumenInversion([FromQuery] int year, [FromQuery] int month)
    {
        var total = await _adsService.ObtenerInversionTotalPorMesAsync(year, month);
        return Ok(new { year, month, total });
    }

    [Authorize(Roles = "admin,superadmin")]
    [HttpDelete("ads/{id}")]
    [SwaggerOperation(Summary = "Eliminar reporte publicitario")]
    public async Task<IActionResult> DeleteAd(int id)
    {
        var deleted = await _adsService.EliminarAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [Authorize(Roles = "viewer,admin,superadmin")]
    [HttpGet("ads/filtrar")]
    [SwaggerOperation(Summary = "Filtrar reportes por tipo, año y mes")]
    public async Task<IActionResult> GetAdsByTipo(
        [FromQuery] TipoPublicidadML tipo,
        [FromQuery] int year,
        [FromQuery] int month)
    {
        var result = await _adsService.ObtenerPorTipoYMesAsync(tipo, year, month);
        return Ok(result);
    }

    #endregion

    #region procesar excel
    [Authorize(Roles = "admin,superadmin")]
    [HttpPost("excel/top-productos")]
    [SwaggerOperation(Summary = "Procesar archivo Excel y obtener top productos por color")]
    public async Task<IActionResult> ProcesarExcel(IFormFile archivo, [FromQuery] int top = 10)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest("El archivo es inválido o está vacío.");

        var resultado = await _excelProcessor.ObtenerTopProductosPorColorAsync(archivo, top);
        return Ok(resultado);
    }

    [Authorize(Roles = "admin,superadmin")]
    [HttpPost("excel/persistir")]
    [SwaggerOperation(Summary = "Procesar archivo Excel y guardar el top de productos por color en la base de datos")]
    public async Task<IActionResult> GuardarAnalisisExcel([FromForm] SaveExcelTopDto dto)
    {
        await _excelProcessor.GuardarAnalisisAsync(dto);
        return Ok("Análisis guardado correctamente.");
    }

    [Authorize(Roles = "viewer,admin,superadmin")]
    [HttpGet("excel/analisis")]
    [SwaggerOperation(Summary = "Obtener el top de productos por color previamente guardado para un mes y año")]
    public async Task<IActionResult> GetAnalisisPorMes([FromQuery] int year, [FromQuery] int month)
    {
        var result = await _excelProcessor.ObtenerAnalisisPorMesAsync(year, month);
        return Ok(result);
    }

    [Authorize(Roles = "viewer,admin,superadmin")]
    [HttpGet("excel/historico")]
    [SwaggerOperation(Summary = "Obtener el historial completo de todos los análisis Excel guardados")]
    public async Task<IActionResult> GetAnalisisHistorico()
    {
        var result = await _excelProcessor.ObtenerHistoricoAsync();
        return Ok(result);
    }
    #endregion
}
