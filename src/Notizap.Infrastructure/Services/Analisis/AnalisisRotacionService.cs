using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;

public class AnalisisRotacionService
{
    private static readonly Dictionary<string, string> PuntosDeVentaComerciales = new Dictionary<string, string>
    {
        { "0001", "General Paz" },
        { "0096", "General Paz" },
        { "0002", "Dean Funes" },
        { "0005", "Dean Funes" },
        { "0095", "Dean Funes" },
        { "0003", "Peatonal" },
        { "0020", "Peatonal" },
        { "0004", "Nueva Cordoba" },
        { "0092", "Nueva Cordoba" },
        { "0006", "E-Commerce" },
        { "0098", "E-Commerce" },
        { "0099", "Casa Central" },
        { "0007", "Casa Central" }
    };
    
  public List<VentaDto> LeerVentas(IFormFile archivoVentas)
  {
    var ventas = new List<VentaDto>();
    var utilitarios = new[] { "AJUSTE POR", "DESCUENTO POR", "BONIFICACION POR", "REDONDEO", "PROMOCION", "GENERICO" };
    var categoriasIgnoradas = new HashSet<string> {
    "BANDOLERAS", "MOCHILAS", "MOCHILAS ESPALDA", "CARTERAS", "MOCHILAS CARRO", 
    "BILLETERAS", "BOTELLITAS", "CARTUCHERAS", "PERFUMERIA", "PORTACOSMETICOS", "RIÑONERAS", "INDUMENTARIA"
    };

    using (var stream = archivoVentas.OpenReadStream())
    using (var workbook = new XLWorkbook(stream))
    {
      var ws = workbook.Worksheet(1);
      var filaHeader = EncontrarFilaHeader(ws, new[] { "FECHA", "NRO", "PRODUCTO", "CANT" });
      if (filaHeader == -1)
        throw new Exception("No se encontró encabezado válido en ventas.");

      var fechaCol = EncontrarColumna(ws, filaHeader, "FECHA");
      var nroCol = EncontrarColumna(ws, filaHeader, "NRO");
      var productoCol = EncontrarColumna(ws, filaHeader, "PRODUCTO");
      var cantidadCol = EncontrarColumna(ws, filaHeader, "CANT");
      var categoriaCol = EncontrarColumna(ws, filaHeader, "CATEGORIA");

      for (int row = filaHeader + 1; row <= ws.LastRowUsed()!.RowNumber(); row++)
      {
        var productoFull = ws.Cell(row, productoCol).GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(productoFull)) continue;

        if (utilitarios.Any(u => productoFull.ToUpper().Contains(u))) continue;

        var partes = productoFull.Split(" - ");
        var productoBase = partes[0].Trim();
        string? color = partes.Length > 1 ? partes[1].Trim() : null;

        var categoria = ws.Cell(row, categoriaCol).GetString()?.Trim().ToUpper();
        if (!string.IsNullOrWhiteSpace(categoria) && categoriasIgnoradas.Contains(categoria))
        continue;

        var nroFactura = ws.Cell(row, nroCol).GetString()?.Trim();
        string? puntoDeVenta = null;
        if (!string.IsNullOrEmpty(nroFactura))
        {
          var nroSplit = nroFactura.Split("-");
          if (nroSplit.Length > 1)
            puntoDeVenta = nroSplit[1];
        }

        // Fecha (toma solo el día)
        var fechaStr = ws.Cell(row, fechaCol).GetString();
        DateTime fecha = DateTime.MinValue;
        if (!string.IsNullOrEmpty(fechaStr))
          DateTime.TryParse(fechaStr.Split(' ')[0], out fecha); // Solo la parte de la fecha

        // Cantidad (puede ser negativa por devolución, bonificación, etc.)
        int cantidad = int.TryParse(ws.Cell(row, cantidadCol).GetString(), out var cant) ? cant : 0;

        ventas.Add(new VentaDto
        {
          Fecha = fecha,
          Nro = nroFactura,
          Producto = productoBase,
          Color = color,
          PuntoDeVenta = puntoDeVenta,
          Cantidad = cantidad
        });
      }
    }
    return ventas;
  }

  private int EncontrarFilaHeader(IXLWorksheet ws, string[] claves)
    {
        for (int row = 1; row <= 20; row++) // Busca en las primeras 20 filas
        {
            var rowCells = ws.Row(row).Cells().Select(c => c.GetString().ToUpper()).ToList();
            if (claves.All(clave => rowCells.Any(c => c.Contains(clave))))
                return row;
        }
        return -1;
    }
        private int EncontrarColumna(IXLWorksheet ws, int headerRow, string nombreCol)
    {
        var cells = ws.Row(headerRow).Cells();
        foreach (var cell in cells)
        {
            if (cell.GetString().ToUpper().Contains(nombreCol.ToUpper()))
                return cell.Address.ColumnNumber;
        }
        throw new Exception($"No se encontró la columna '{nombreCol}' en el header.");
    }
    public List<RotacionProductoColorDto> CalcularRotacion(
        List<CompraDetalleConFechaDto> compras,
        List<VentaDto> ventas)
    {
        // Sumar cantidades compradas solo por producto base
        var cantidadesCompradas = CalcularCantidadCompradaPorProducto(compras);

        // Agrupar ventas por producto base, color y punto de venta
        var ventasAgrupadas = ventas
            .GroupBy(v => new {
                Producto = v.Producto!.Trim().ToUpper(),
                Color = (v.Color ?? "").Trim().ToUpper(),
                PuntoDeVenta = v.PuntoDeVenta != null && PuntosDeVentaComerciales.ContainsKey(v.PuntoDeVenta.Trim())
                    ? PuntosDeVentaComerciales[v.PuntoDeVenta.Trim()]
                    : (v.PuntoDeVenta ?? "")
            })
            .Select(g => new {
                Producto = g.Key.Producto,
                Color = g.Key.Color,
                PuntoDeVenta = g.Key.PuntoDeVenta,
                CantidadVendida = g.Sum(v => v.Cantidad)
            })
            .ToList();

        var resultado = new List<RotacionProductoColorDto>();

        foreach (var venta in ventasAgrupadas)
        {
            // Buscamos la cantidad comprada por producto base
            cantidadesCompradas.TryGetValue(venta.Producto, out int cantidadComprada);

            resultado.Add(new RotacionProductoColorDto
            {
                Producto = venta.Producto,
                Color = string.IsNullOrWhiteSpace(venta.Color) ? null : venta.Color,
                PuntoDeVenta = venta.PuntoDeVenta,
                CantidadComprada = cantidadComprada,
                CantidadVendida = venta.CantidadVendida
            });
        }
        return resultado;
    }

    // Reporte de productos vendidos sin compras
    public List<VentaSinCompraDto> VentasSinCompras(
        List<CompraDetalleConFechaDto> compras,
        List<VentaDto> ventas)
    {
        var productosComprados = compras.Select(c => c.Producto!.Trim().ToUpper()).Distinct().ToHashSet();

        return ventas
            .Where(v => !productosComprados.Contains(v.Producto!.Trim().ToUpper()))
            .GroupBy(v => new {
                Producto = v.Producto!.Trim().ToUpper(),
                Color = (v.Color ?? "").Trim().ToUpper(),
                PuntoDeVenta = v.PuntoDeVenta
            })
            .Select(g => new VentaSinCompraDto
            {
                Producto = g.Key.Producto,
                Color = string.IsNullOrWhiteSpace(g.Key.Color) ? null : g.Key.Color,
                PuntoDeVenta = g.Key.PuntoDeVenta,
                CantidadVendida = g.Sum(v => v.Cantidad)
            })
            .ToList();
    }
    public Dictionary<string, int> CalcularCantidadCompradaPorProducto(List<CompraDetalleConFechaDto> compras)
    {
        return compras
            .GroupBy(c => c.Producto!.Trim().ToUpper())
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x =>
                {
                    int cantidad = x.Cantidad;
                    return cantidad > 0 ? cantidad : 0;
                })
            );
    }
}
