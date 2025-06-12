using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/analisis/rotacion")]
public class AnalisisRotacionController : ControllerBase
{
    private readonly IComprasMergeService _comprasMergeService;
    private readonly AnalisisRotacionService _analisisRotacionService;
    private readonly IEvolucionStockService _evolucionStockService;
    private readonly IEvolucionVentasService _evolucionVentasService;

    public AnalisisRotacionController(
        IComprasMergeService comprasMergeService,
        AnalisisRotacionService analisisRotacionService,
        IEvolucionStockService evolucionStockService,
        IEvolucionVentasService evolucionVentasService)
    {
        _comprasMergeService = comprasMergeService;
        _analisisRotacionService = analisisRotacionService;
        _evolucionStockService = evolucionStockService;
        _evolucionVentasService = evolucionVentasService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(AnalisisRotacionResponse), 200)]
    [SwaggerOperation(Summary = "Calcula la tasa de rotacion de los productos de los archivos subidos")]
    public IActionResult AnalizarRotacion([FromForm] AnalisisRotacionRequest request)
    {
        if (request.ArchivoComprasCabecera == null || request.ArchivoComprasDetalles == null || request.ArchivoVentas == null)
            return BadRequest("Debes adjuntar los tres archivos .xlsx (cabecera de compras, detalles de compras y ventas)");

        var compras =  _comprasMergeService.MergeComprasConDetalles(
            request.ArchivoComprasCabecera, request.ArchivoComprasDetalles);

        var ventas =  _analisisRotacionService.LeerVentas(request.ArchivoVentas);

        var rotacion = _analisisRotacionService.CalcularRotacion(compras, ventas);
        var ventasSinCompras = _analisisRotacionService.VentasSinCompras(compras, ventas);

        return Ok(new AnalisisRotacionResponse
        {
            Rotacion = rotacion,
            VentasSinCompras = ventasSinCompras
        });
    }
  [HttpPost("evolucion-stock")]
  [ProducesResponseType(typeof(List<EvolucionStockPorPuntoDeVentaDto>), 200)]
  [SwaggerOperation(Summary = "Evolucion de stock diaria de los productos de los archivos subidos")]
  public IActionResult EvolucionStock([FromForm] EvolucionStockRequest request)
  {
    if (request.ArchivoComprasCabecera == null || request.ArchivoComprasDetalles == null || request.ArchivoVentas == null)
      return BadRequest("Debes adjuntar los tres archivos .xlsx (cabecera de compras, detalles de compras y ventas)");
    if (string.IsNullOrWhiteSpace(request.Producto))
      return BadRequest("Debes especificar el producto base a analizar.");

    var compras = _comprasMergeService.MergeComprasConDetalles(
        request.ArchivoComprasCabecera, request.ArchivoComprasDetalles);

    var ventas = _analisisRotacionService.LeerVentas(request.ArchivoVentas);

    try
    {
      var evolucion = _evolucionStockService.CalcularEvolucionStock(compras, ventas, request.Producto.Trim());
      return Ok(evolucion);
    }
    catch (Exception ex)
    {
      return BadRequest(ex.Message);
    }
  }

  [HttpPost("evolucion-ventas")]
  [ProducesResponseType(typeof(EvolucionVentasResponse), 200)]
  [SwaggerOperation(Summary = "Devuelve la evolución diaria de ventas de todos los productos")]
  public IActionResult EvolucionVentas([FromForm] EvolucionVentasRequest request)
  {
      if (request.ArchivoVentas == null)
        return BadRequest("Debes adjuntar el archivo de ventas (.xlsx)");

      try
      {
          var resultado = _evolucionVentasService.CalcularEvolucionVentas(request.ArchivoVentas);
          return Ok(resultado);
      }
      catch (Exception ex)
      {
          return BadRequest(ex.Message);
      }
  }

  // helper
  [HttpPost("compras/fechas-compra")]
  public ActionResult<List<string>> GetFechasCompra([FromForm] FechasCompraRequest request)
  {
    var compras = _comprasMergeService.MergeComprasConDetalles(request.ArchivoCabecera, request.ArchivoDetalles);
    var productoNorm = request.Producto.Trim().ToUpper();
    var fechas = compras
        .Where(c => c.Producto != null && c.Producto.Trim().ToUpper() == productoNorm)
        .Select(c => c.Fecha)
        .Distinct()
        .Where(f => !string.IsNullOrWhiteSpace(f))
        .OrderBy(f => f)
        .Select(f => DateTime.Parse(f!.Split(' ')[0]).ToString("yyyy-MM-dd"))
        .ToList();

    return Ok(fechas);
  }
  
  [HttpPost("evolucion-ventas/resumen")]
  [ProducesResponseType(typeof(EvolucionVentasResumenResponse), 200)]
  [SwaggerOperation(Summary = "Devuelve la evolución de ventas agregada por sucursal para gráficos")]
  public IActionResult EvolucionVentasResumen([FromForm] EvolucionVentasRequest request)
  {
      if (request.ArchivoVentas == null)
          return BadRequest("Debes adjuntar el archivo de ventas (.xlsx)");

      try
      {
          var resultado = _evolucionVentasService.CalcularEvolucionVentasResumen(request.ArchivoVentas);
          return Ok(resultado);
      }
      catch (Exception ex)
      {
          return BadRequest(ex.Message);
      }
  }
}
