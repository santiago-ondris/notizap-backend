using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "superadmin")]
[ApiController]
[Route("api/v1/ventas-vendedoras")]
public class VentasVendedorasController : ControllerBase
{
    private readonly IVentaVendedoraService _ventaVendedoraService;
    private readonly ILogger<VentasVendedorasController> _logger;

    public VentasVendedorasController(
        IVentaVendedoraService ventaVendedoraService,
        ILogger<VentasVendedorasController> logger)
    {
        _ventaVendedoraService = ventaVendedoraService;
        _logger = logger;
    }

    /// <summary>
    /// Subir archivo Excel con datos de ventas de vendedoras
    /// </summary>
    [HttpPost("upload")]
    public async Task<ActionResult> SubirArchivoVentas([FromForm] VentaVendedoraUploadDto uploadDto)
    {
        try
        {
            if (uploadDto.Archivo == null || uploadDto.Archivo.Length == 0)
            {
                return BadRequest(new { message = "Debe seleccionar un archivo válido." });
            }

            // Validar extensión del archivo
            var extension = Path.GetExtension(uploadDto.Archivo.FileName).ToLowerInvariant();
            if (extension != ".xlsx")
            {
                return BadRequest(new { message = "El archivo debe ser un Excel en formato .xlsx (no se admite .xls)." });
            }

            // Validar tamaño (máximo 10MB)
            if (uploadDto.Archivo.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new { message = "El archivo no puede exceder los 10MB." });
            }

            using var stream = uploadDto.Archivo.OpenReadStream();
            var resultado = await _ventaVendedoraService.SubirArchivoVentasAsync(
                stream, uploadDto.SobreescribirDuplicados);

            if (resultado.Success)
            {
                _logger.LogInformation($"Archivo de ventas subido exitosamente: {uploadDto.Archivo.FileName}");
                return Ok(new 
                { 
                    message = resultado.Message,
                    stats = resultado.Stats
                });
            }

            return BadRequest(new { message = resultado.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir archivo de ventas");
            return StatusCode(500, new { message = "Error interno del servidor al procesar el archivo." });
        }
    }

    /// <summary>
    /// Validar archivo sin procesarlo completamente
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult> ValidarArchivo([FromForm] ArchivoDto dto)
    {
        try
        {
            if (dto.Archivo == null || dto.Archivo.Length == 0)
            {
                return BadRequest(new { message = "Debe seleccionar un archivo válido." });
            }

            using var stream = dto.Archivo.OpenReadStream();
            var errores = await _ventaVendedoraService.ValidarArchivoSinProcesarAsync(stream);

            return Ok(new { errores, esValido = !errores.Any() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar archivo");
            return StatusCode(500, new { message = "Error al validar el archivo." });
        }
    }

    /// <summary>
    /// Obtener ventas con filtros y paginación
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<VentaVendedoraDto>>> ObtenerVentas([FromQuery] VentaVendedoraFilterDto filtros)
    {
        try
        {
            var (ventas, totalRegistros) = await _ventaVendedoraService.ObtenerVentasAsync(filtros);

            return Ok(new 
            { 
                data = ventas,
                totalRegistros,
                pagina = filtros.Page,
                pageSize = filtros.PageSize,
                totalPaginas = (int)Math.Ceiling((double)totalRegistros / filtros.PageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas");
            return StatusCode(500, new { message = "Error al obtener las ventas." });
        }
    }

    /// <summary>
    /// Obtener estadísticas generales de ventas
    /// </summary>
    [HttpGet("estadisticas")]
    public async Task<ActionResult<VentaVendedoraStatsDto>> ObtenerEstadisticas([FromQuery] VentaVendedoraFilterDto filtros)
    {
        try
        {
            var stats = await _ventaVendedoraService.ObtenerEstadisticasAsync(filtros);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas");
            return StatusCode(500, new { message = "Error al obtener las estadísticas." });
        }
    }

    /// <summary>
    /// Obtener ventas agrupadas por día
    /// </summary>
    [HttpGet("por-dia")]
    public async Task<ActionResult<List<VentaPorDiaDto>>> ObtenerVentasPorDia([FromQuery] VentaVendedoraFilterDto filtros)
    {
        try
        {
            var ventasPorDia = await _ventaVendedoraService.ObtenerVentasPorDiaAsync(filtros);
            return Ok(ventasPorDia);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas por día");
            return StatusCode(500, new { message = "Error al obtener ventas por día." });
        }
    }

    /// <summary>
    /// Obtener top vendedoras por rendimiento
    /// </summary>
    [HttpGet("top-vendedoras")]
    public async Task<ActionResult<List<VentaPorVendedoraDto>>> ObtenerTopVendedoras(
        [FromQuery] VentaVendedoraFilterDto filtros, [FromQuery] int top = 10)
    {
        try
        {
            var topVendedoras = await _ventaVendedoraService.ObtenerTopVendedorasAsync(filtros, top);
            return Ok(topVendedoras);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener top vendedoras");
            return StatusCode(500, new { message = "Error al obtener top vendedoras." });
        }
    }

    /// <summary>
    /// Obtener ventas agrupadas por sucursal
    /// </summary>
    [HttpGet("por-sucursal")]
    public async Task<ActionResult<List<VentaPorSucursalDto>>> ObtenerVentasPorSucursal([FromQuery] VentaVendedoraFilterDto filtros)
    {
        try
        {
            var ventasPorSucursal = await _ventaVendedoraService.ObtenerVentasPorSucursalAsync(filtros);
            return Ok(ventasPorSucursal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas por sucursal");
            return StatusCode(500, new { message = "Error al obtener ventas por sucursal." });
        }
    }

    /// <summary>
    /// Obtener ventas agrupadas por turno
    /// </summary>
    [HttpGet("por-turno")]
    public async Task<ActionResult<List<VentaPorTurnoDto>>> ObtenerVentasPorTurno([FromQuery] VentaVendedoraFilterDto filtros)
    {
        try
        {
            var ventasPorTurno = await _ventaVendedoraService.ObtenerVentasPorTurnoAsync(filtros);
            return Ok(ventasPorTurno);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas por turno");
            return StatusCode(500, new { message = "Error al obtener ventas por turno." });
        }
    }

    /// <summary>
    /// Obtener lista de sucursales disponibles
    /// </summary>
    [HttpGet("sucursales")]
    public async Task<ActionResult<List<string>>> ObtenerSucursales()
    {
        try
        {
            var sucursales = await _ventaVendedoraService.ObtenerSucursalesAsync();
            return Ok(sucursales);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener sucursales");
            return StatusCode(500, new { message = "Error al obtener sucursales." });
        }
    }

    /// <summary>
    /// Obtener lista de vendedoras disponibles
    /// </summary>
    [HttpGet("vendedores")]
    public async Task<ActionResult<List<string>>> ObtenerVendedores()
    {
        try
        {
            var vendedores = await _ventaVendedoraService.ObtenerVendedoresAsync();
            return Ok(vendedores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener vendedores");
            return StatusCode(500, new { message = "Error al obtener vendedores." });
        }
    }

    /// <summary>
    /// Obtener rango de fechas disponibles en la base de datos
    /// </summary>
    [HttpGet("rango-fechas")]
    public async Task<ActionResult> ObtenerRangoFechas()
    {
        try
        {
            var (fechaMinima, fechaMaxima) = await _ventaVendedoraService.ObtenerRangoFechasAsync();
            var (fechaInicioSemana, fechaFinSemana) = await _ventaVendedoraService.ObtenerUltimaSemanaConDatosAsync();

            return Ok(new 
            { 
                fechaMinima,
                fechaMaxima,
                ultimaSemana = new 
                {
                    fechaInicio = fechaInicioSemana,
                    fechaFin = fechaFinSemana
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener rango de fechas");
            return StatusCode(500, new { message = "Error al obtener rango de fechas." });
        }
    }

    /// <summary>
    /// Verificar si existen datos en un rango de fechas
    /// </summary>
    [HttpGet("verificar-datos")]
    public async Task<ActionResult> VerificarDatosEnRango([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
    {
        try
        {
            var existenDatos = await _ventaVendedoraService.ExistenDatosEnRangoFechasAsync(fechaInicio, fechaFin);
            return Ok(new { existenDatos });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar datos en rango");
            return StatusCode(500, new { message = "Error al verificar datos." });
        }
    }

    /// <summary>
    /// Eliminar ventas en un rango de fechas específico
    /// </summary>
    [HttpDelete("eliminar-rango")]
    public async Task<ActionResult> EliminarVentasPorRango([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
    {
        try
        {
            var eliminado = await _ventaVendedoraService.EliminarVentasPorRangoFechasAsync(fechaInicio, fechaFin);
            
            if (eliminado)
            {
                _logger.LogInformation($"Ventas eliminadas del rango {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}");
                return Ok(new { message = $"Ventas eliminadas del período {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}" });
            }

            return NotFound(new { message = "No se encontraron datos para eliminar en el rango especificado." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar ventas por rango");
            return StatusCode(500, new { message = "Error al eliminar las ventas." });
        }
    }

    /// <summary>
    /// Obtener todas las vendedoras (no limitado como top-vendedoras)
    /// </summary>
    [HttpGet("todas-vendedoras")]
    public async Task<ActionResult<List<VentaPorVendedoraDto>>> ObtenerTodasLasVendedoras([FromQuery] VentaVendedoraFilterDto filtros)
    {
        try
        {
            var todasVendedoras = await _ventaVendedoraService.ObtenerTodasLasVendedorasAsync(filtros);
            return Ok(todasVendedoras);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todas las vendedoras");
            return StatusCode(500, new { message = "Error al obtener todas las vendedoras." });
        }
    }
}