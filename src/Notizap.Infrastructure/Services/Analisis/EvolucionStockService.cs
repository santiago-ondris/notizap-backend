public class EvolucionStockService : IEvolucionStockService
{
    private static readonly Dictionary<string, string> PuntosDeVentaComerciales = new Dictionary<string, string>
    {
        { "0001", "General Paz" },
        { "0002", "Dean Funes" },
        { "0005", "Dean Funes" },
        { "0003", "Peatonal" },
        { "0004", "Nueva Cordoba" },
        { "0006", "E-Commerce" }
    };
    public List<EvolucionStockPorPuntoDeVentaDto> CalcularEvolucionStock(
        List<CompraDetalleConFechaDto> comprasOriginal,
        List<VentaDto> ventas,
        string productoBase
    )
    {
        var compras = comprasOriginal
            .Where(c => !string.IsNullOrWhiteSpace(c.Producto) && !string.IsNullOrWhiteSpace(c.Fecha))
            .Select(c => new CompraSimpleStockDto
            {
                Producto = c.Producto,
                Cantidad = c.Cantidad,
                Fecha = ParsearFechaExcel(c.Fecha)
            })
            .Where(c => c.Fecha != DateTime.MinValue)
            .ToList();

        var productoBaseNorm = productoBase.Trim().ToUpper();

        var comprasFiltradas = compras
            .Where(c => c.Producto != null && c.Producto.Trim().ToUpper() == productoBaseNorm)
            .ToList();

        if (!comprasFiltradas.Any())
            throw new Exception("No se encontraron compras para ese producto base.");

        var ventasFiltradas = ventas
            .Where(v => !string.IsNullOrEmpty(v.Producto) && v.Producto.Trim().ToUpper() == productoBaseNorm)
            .ToList();

        var fechaInicio = comprasFiltradas.Min(c => c.Fecha.Date);
        var fechaFin = ventasFiltradas.Any() ? ventasFiltradas.Max(v => v.Fecha.Date) : fechaInicio;

        var dias = Enumerable.Range(0, (fechaFin - fechaInicio).Days + 1)
            .Select(offset => fechaInicio.AddDays(offset))
            .ToList();

        // ---- EVOLUCIÓN GLOBAL ----
        var evolucionGlobal = CalcularEvolucionPorFechas(
            dias,
            comprasFiltradas,
            ventasFiltradas,
            null!
        );

        var resultado = new List<EvolucionStockPorPuntoDeVentaDto>
        {
            new EvolucionStockPorPuntoDeVentaDto
            {
                PuntoDeVenta = "GLOBAL",
                Evolucion = evolucionGlobal
            }
        };

        // ---- EVOLUCIÓN POR PUNTO DE VENTA ----
        var puntosDeVentaAgrupados = ventasFiltradas
            .Where(v => !string.IsNullOrWhiteSpace(v.PuntoDeVenta))
            .GroupBy(v => PuntosDeVentaComerciales.ContainsKey(v.PuntoDeVenta!.Trim()) 
                ? PuntosDeVentaComerciales[v.PuntoDeVenta.Trim()]
                : v.PuntoDeVenta.Trim())
            .ToList();

        foreach (var grupo in puntosDeVentaAgrupados)
        {
            var nombreComercial = grupo.Key;
            var ventasPdv = grupo.ToList();

            var evolucionPdv = CalcularEvolucionPorFechas(
                dias,
                comprasFiltradas,
                ventasPdv,
                null!
            );

            resultado.Add(new EvolucionStockPorPuntoDeVentaDto
            {
                PuntoDeVenta = nombreComercial,
                Evolucion = evolucionPdv
            });
        }

        return resultado;
    }
    private List<EvolucionStockDiaDto> CalcularEvolucionPorFechas(
        List<DateTime> dias,
        List<CompraSimpleStockDto> compras,
        List<VentaDto> ventas,
        string puntoDeVenta 
    )
    {
        var evolucion = new List<EvolucionStockDiaDto>();
        int stock = 0;

        var comprasPorFecha = compras
            .GroupBy(c => c.Fecha.Date)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Cantidad));

        var ventasPorFecha = ventas
            .Where(v => puntoDeVenta == null || (v.PuntoDeVenta != null && v.PuntoDeVenta.Trim() == puntoDeVenta))
            .GroupBy(v => v.Fecha.Date)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Cantidad));

        foreach (var dia in dias)
        {
            if (comprasPorFecha.ContainsKey(dia))
                stock += comprasPorFecha[dia];

            if (ventasPorFecha.ContainsKey(dia))
                stock -= ventasPorFecha[dia];

            evolucion.Add(new EvolucionStockDiaDto
            {
                Fecha = dia,
                Stock = stock
            });
        }

        return evolucion;
    }

    private DateTime ParsearFechaExcel(string? fechaStr)
    {
        if (string.IsNullOrWhiteSpace(fechaStr))
            return DateTime.MinValue;

        var soloFecha = fechaStr.Split(' ')[0];
        if (DateTime.TryParse(soloFecha, out var fecha))
            return fecha;
        return DateTime.MinValue;
    }
}
