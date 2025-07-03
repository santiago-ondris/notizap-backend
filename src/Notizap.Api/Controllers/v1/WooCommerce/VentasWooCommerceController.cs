using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/ventas-woocommerce")]
[Authorize] // Requiere autenticación para todos los endpoints
public class VentasWooCommerceController : ControllerBase
{
    private readonly IVentaWooCommerceService _ventaService;

    public VentasWooCommerceController(IVentaWooCommerceService ventaService)
    {
        _ventaService = ventaService;
    }

    /// <summary>
    /// Obtiene una venta por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<VentaWooCommerceDto>> GetById(int id)
    {
        try
        {
            var venta = await _ventaService.GetByIdAsync(id);
            return Ok(venta);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene todas las ventas con paginación y filtros
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<object>> GetPaged([FromQuery] VentaWooCommerceQueryDto query)
    {
        try
        {
            var (items, totalCount) = await _ventaService.GetPagedAsync(query);
            
            return Ok(new
            {
                items,
                totalCount,
                pageNumber = query.PageNumber,
                pageSize = query.PageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Crea una nueva venta (Solo admin y superadmin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<ActionResult<VentaWooCommerceDto>> Create([FromBody] CreateVentaWooCommerceDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var venta = await _ventaService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = venta.Id }, venta);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza una venta existente (Solo admin y superadmin)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<ActionResult<VentaWooCommerceDto>> Update(int id, [FromBody] UpdateVentaWooCommerceDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != updateDto.Id)
                return BadRequest(new { message = "El ID de la URL no coincide con el ID del cuerpo" });

            var venta = await _ventaService.UpdateAsync(updateDto);
            return Ok(venta);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Elimina una venta por ID (Solo admin y superadmin)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _ventaService.DeleteAsync(id);
            
            if (!deleted)
                return NotFound(new { message = $"Venta con ID {id} no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene una venta específica por tienda y período
    /// </summary>
    [HttpGet("tienda/{tienda}/periodo/{mes}/{año}")]
    public async Task<ActionResult<VentaWooCommerceDto>> GetByTiendaYPeriodo(string tienda, int mes, int año)
    {
        try
        {
            var venta = await _ventaService.GetByTiendaYPeriodoAsync(tienda, mes, año);
            
            if (venta == null)
                return NotFound(new { message = $"No se encontró venta para {tienda} en {mes:D2}/{año}" });

            return Ok(venta);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene todas las ventas de un período específico
    /// </summary>
    [HttpGet("periodo/{mes}/{año}")]
    public async Task<ActionResult<IEnumerable<VentaWooCommerceDto>>> GetByPeriodo(int mes, int año)
    {
        try
        {
            var ventas = await _ventaService.GetByPeriodoAsync(mes, año);
            return Ok(ventas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene todas las ventas de una tienda
    /// </summary>
    [HttpGet("tienda/{tienda}")]
    public async Task<ActionResult<IEnumerable<VentaWooCommerceDto>>> GetByTienda(string tienda)
    {
        try
        {
            var ventas = await _ventaService.GetByTiendaAsync(tienda);
            return Ok(ventas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene todas las ventas de un año
    /// </summary>
    [HttpGet("año/{año}")]
    public async Task<ActionResult<IEnumerable<VentaWooCommerceDto>>> GetByAño(int año)
    {
        try
        {
            var ventas = await _ventaService.GetByAñoAsync(año);
            return Ok(ventas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Dashboard: Obtiene totales por período (como tu Excel)
    /// </summary>
    [HttpGet("dashboard/totales/{mes}/{año}")]
    public async Task<ActionResult<TotalesVentasDto>> GetTotalesByPeriodo(int mes, int año)
    {
        try
        {
            var totales = await _ventaService.GetTotalesByPeriodoAsync(mes, año);
            return Ok(totales);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Dashboard: Obtiene resumen por período
    /// </summary>
    [HttpGet("dashboard/resumen/{mes}/{año}")]
    public async Task<ActionResult<IEnumerable<ResumenVentasDto>>> GetResumenByPeriodo(int mes, int año)
    {
        try
        {
            var resumen = await _ventaService.GetResumenByPeriodoAsync(mes, año);
            return Ok(resumen);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Dashboard: Obtiene totales de todo un año
    /// </summary>
    [HttpGet("dashboard/año/{año}")]
    public async Task<ActionResult<IEnumerable<TotalesVentasDto>>> GetTotalesByAño(int año)
    {
        try
        {
            var totales = await _ventaService.GetTotalesByAñoAsync(año);
            return Ok(totales);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Verifica si existe una venta para tienda y período
    /// </summary>
    [HttpGet("exists/tienda/{tienda}/periodo/{mes}/{año}")]
    public async Task<ActionResult<bool>> Exists(string tienda, int mes, int año)
    {
        try
        {
            var exists = await _ventaService.ExistsAsync(tienda, mes, año);
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene estadísticas de una tienda en un año
    /// </summary>
    [HttpGet("estadisticas/tienda/{tienda}/año/{año}")]
    public async Task<ActionResult<object>> GetEstadisticasTienda(string tienda, int año)
    {
        try
        {
            var totalFacturado = await _ventaService.GetTotalFacturadoByTiendaAsync(tienda, año);
            var totalUnidades = await _ventaService.GetTotalUnidadesByTiendaAsync(tienda, año);

            return Ok(new
            {
                tienda,
                año,
                totalFacturado,
                totalUnidades
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene listas auxiliares para dropdowns
    /// </summary>
    [HttpGet("auxiliares")]
    public async Task<ActionResult<object>> GetAuxiliares()
    {
        try
        {
            var tiendas = await _ventaService.GetTiendasDisponiblesAsync();
            var años = await _ventaService.GetAñosDisponiblesAsync();

            return Ok(new
            {
                tiendas,
                años
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Crea múltiples ventas en lote (Solo admin y superadmin)
    /// </summary>
    [HttpPost("batch")]
    [Authorize(Roles = "admin,superadmin")]
    public async Task<ActionResult<IEnumerable<VentaWooCommerceDto>>> CreateBatch([FromBody] IEnumerable<CreateVentaWooCommerceDto> createDtos)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ventas = await _ventaService.CreateBatchAsync(createDtos);
            return Ok(ventas);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Elimina todas las ventas de un período (Solo superadmin)
    /// </summary>
    [HttpDelete("periodo/{mes}/{año}")]
    [Authorize(Roles = "superadmin")]
    public async Task<ActionResult> DeleteByPeriodo(int mes, int año)
    {
        try
        {
            var deleted = await _ventaService.DeleteByPeriodoAsync(mes, año);
            
            if (!deleted)
                return NotFound(new { message = $"No se encontraron ventas para el período {mes:D2}/{año}" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Calcula crecimiento mensual de una tienda
    /// </summary>
    [HttpGet("crecimiento/tienda/{tienda}/actual/{mesActual}/{añoActual}/anterior/{mesAnterior}/{añoAnterior}")]
    public async Task<ActionResult<object>> GetCrecimientoMensual(
        string tienda, int mesActual, int añoActual, int mesAnterior, int añoAnterior)
    {
        try
        {
            var crecimiento = await _ventaService.GetCrecimientoMensualAsync(
                tienda, mesActual, añoActual, mesAnterior, añoAnterior);

            return Ok(new
            {
                tienda,
                periodoActual = $"{mesActual:D2}/{añoActual}",
                periodoAnterior = $"{mesAnterior:D2}/{añoAnterior}",
                crecimientoPorcentual = crecimiento
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene evolución anual de una tienda
    /// </summary>
    [HttpGet("evolucion/tienda/{tienda}/año/{año}")]
    public async Task<ActionResult<IEnumerable<TotalesVentasDto>>> GetEvolucionAnual(string tienda, int año)
    {
        try
        {
            var evolucion = await _ventaService.GetEvolucionAnualAsync(tienda, año);
            return Ok(evolucion);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }
}