public static class RendimientoLocalesHelper
{
    public static List<RendimientoLocalesDiaDto> CalcularRendimientoPorDia(
        List<VentaVendedoraDto> ventas,
        string sucursalNombre,
        string? turno)
    {
        // Filtrar solo por sucursal
        var ventasFiltradas = ventas
            .Where(v => v.SucursalNombre == sucursalNombre)
            .ToList();

        // Agrupar por día y turno
        var diasAgrupados = ventasFiltradas
            .GroupBy(v => new { Fecha = v.Fecha.Date, v.Turno })
            .Select(g =>
            {
                var montoTotal = g.Sum(v => v.Cantidad < 0 ? -v.Total : v.Total);
                var cantidadTotal = g.Sum(v => v.EsProductoDescuento && v.Cantidad == -1 ? 0 : v.Cantidad);

                var vendedorasAgrupadas = g
                    .GroupBy(v => v.VendedorNombre)
                    .Select(vg =>
                    {
                        var monto = vg.Sum(v => v.Cantidad < 0 ? -v.Total : v.Total);
                        var cantidad = vg.Sum(v => v.EsProductoDescuento && v.Cantidad == -1 ? 0 : v.Cantidad);
                        return new RendimientoVendedoraDiaDto
                        {
                            VendedoraNombre = vg.Key,
                            Monto = monto,
                            Cantidad = cantidad,
                            CumplioMontoPromedio = false,
                            CumplioCantidadPromedio = false
                        };
                    })
                    .ToList();

                var promedioMonto = vendedorasAgrupadas.Any() ? montoTotal / vendedorasAgrupadas.Count : 0;
                var promedioCantidad = vendedorasAgrupadas.Any() ? (decimal)cantidadTotal / vendedorasAgrupadas.Count : 0;

                foreach (var v in vendedorasAgrupadas)
                {
                    v.CumplioMontoPromedio = v.Monto >= promedioMonto;
                    v.CumplioCantidadPromedio = v.Cantidad >= promedioCantidad;
                }

                return new RendimientoLocalesDiaDto
                {
                    SucursalNombre = sucursalNombre,
                    Fecha = g.Key.Fecha,
                    Turno = g.Key.Turno,
                    MontoTotal = montoTotal,
                    CantidadTotal = cantidadTotal,
                    PromedioMontoVendedora = promedioMonto,
                    PromedioCantidadVendedora = promedioCantidad,
                    Vendedoras = vendedorasAgrupadas
                };
            })
            .OrderBy(d => d.Fecha)
            .ToList();

        // Filtrar por turno SI el usuario seleccionó alguno
        if (!string.IsNullOrEmpty(turno))
        {
            diasAgrupados = diasAgrupados
                .Where(d => d.Turno == turno)
                .ToList();
        }

        return diasAgrupados;
    }

    public static List<RendimientoVendedoraResumenDto> CalcularResumenPorVendedora(List<RendimientoLocalesDiaDto> dias, string metrica)
    {
        // metrica: "monto" o "cantidad"
        var resumen = dias
            .SelectMany(d => d.Vendedoras.Select(v => new
            {
                v.VendedoraNombre,
                Cumplio = metrica == "cantidad" ? v.CumplioCantidadPromedio : v.CumplioMontoPromedio,
                d.Fecha
            }))
            .GroupBy(x => x.VendedoraNombre)
            .Select(g =>
            {
                var diasTrabajados = g.Count();
                var diasCumplio = g.Count(x => x.Cumplio);
                return new RendimientoVendedoraResumenDto
                {
                    VendedoraNombre = g.Key,
                    DiasTrabajados = diasTrabajados,
                    DiasCumplioMonto = metrica == "monto" ? diasCumplio : 0,
                    DiasCumplioCantidad = metrica == "cantidad" ? diasCumplio : 0,
                    PorcentajeCumplimientoMonto = metrica == "monto" && diasTrabajados > 0 ? (decimal)diasCumplio * 100 / diasTrabajados : 0,
                    PorcentajeCumplimientoCantidad = metrica == "cantidad" && diasTrabajados > 0 ? (decimal)diasCumplio * 100 / diasTrabajados : 0
                };
            })
            .ToList();

        return resumen;
    }
}
