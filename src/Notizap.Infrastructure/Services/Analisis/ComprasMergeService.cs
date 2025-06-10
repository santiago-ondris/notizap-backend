using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;

public class ComprasMergeService : IComprasMergeService
{
  public List<CompraDetalleConFechaDto> MergeComprasConDetalles(
      IFormFile archivoComprasCabecera,
      IFormFile archivoComprasDetalles
  )
  {
    // 1. Leer archivo de compras (cabecera) y armar diccionario NRO->FECHA
    var comprasCabecera = new Dictionary<string, string>();

    using (var stream = archivoComprasCabecera.OpenReadStream())
    using (var workbook = new XLWorkbook(stream))
    {
      var ws = workbook.Worksheet(1);
      // Buscar la fila de encabezado (donde están NRO y FECHA)
      var filaHeader = ExcelFinder.EncontrarFilaHeader(ws, new[] { "NRO", "FECHA" });

      if (filaHeader == -1)
        throw new Exception("No se encontró encabezado válido en archivo de cabecera de compras.");

      var nroCol = ExcelFinder.EncontrarColumna(ws, filaHeader, "NRO");
      var fechaCol = ExcelFinder.EncontrarColumna(ws, filaHeader, "FECHA");

      for (int row = filaHeader + 1; row <= ws.LastRowUsed()!.RowNumber(); row++)
      {
        var nro = ws.Cell(row, nroCol).GetString()?.Trim();
        var fecha = ws.Cell(row, fechaCol).GetString()?.Trim();

        if (!string.IsNullOrWhiteSpace(nro) && !comprasCabecera.ContainsKey(nro))
        {
          comprasCabecera[nro] = fecha ?? "";
        }
      }
    }

    var resultado = new List<CompraDetalleConFechaDto>();

    using (var stream = archivoComprasDetalles.OpenReadStream())
    using (var workbook = new XLWorkbook(stream))
    {
      var ws = workbook.Worksheet(1);

      // Detectar la fila de encabezado, buscando NRO y PROVEEDOR, CANTIDAD, TOTAL, PRODUCTO
      var filaHeader = ExcelFinder.EncontrarFilaHeader(ws, new[] { "NRO", "PROVEEDOR", "CANT", "TOTAL" });

      if (filaHeader == -1)
        throw new Exception("No se encontró encabezado válido en archivo de detalles de compras.");

      // Encontrar los números de columna
      var nroCol = ExcelFinder.EncontrarColumna(ws, filaHeader, "NRO");
      var proveedorCol = ExcelFinder.EncontrarColumna(ws, filaHeader, "PROVEEDOR");
      var productoCol = ExcelFinder.EncontrarColumna(ws, filaHeader, "PRODUCTO");
      var cantidadCol = ExcelFinder.EncontrarColumna(ws, filaHeader, "CANT");
      var totalCol = ExcelFinder.EncontrarColumna(ws, filaHeader, "TOTAL");

      // Leer los detalles fila a fila
      for (int row = filaHeader + 1; row <= ws.LastRowUsed()!.RowNumber(); row++)
      {
        var nro = ws.Cell(row, nroCol).GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(nro)) continue; // Saltar filas vacías

        var detalle = new CompraDetalleConFechaDto
        {
          Nro = nro,
          Proveedor = ws.Cell(row, proveedorCol).GetString()?.Trim(),
          Producto = ws.Cell(row, productoCol).GetString()?.Trim(),
          Cantidad = int.TryParse(ws.Cell(row, cantidadCol).GetString(), out var cant) ? cant : 0,
          Total = decimal.TryParse(ws.Cell(row, totalCol).GetString(), out var tot) ? tot : 0m,
          Fecha = comprasCabecera.ContainsKey(nro) ? comprasCabecera[nro] : ""
        };

        resultado.Add(detalle);
      }
    }
    return resultado;
  }
}