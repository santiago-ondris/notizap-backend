public class WooResumenBuilder
{
    private readonly IWooCommerceService _wooService;

    public WooResumenBuilder(IWooCommerceService wooService)
    {
        _wooService = wooService;
    }

    public async Task<WooResumenDto> ConstruirAsync(int year, int month)
    {
        var resumen = new WooResumenDto();

        resumen.TotalMontella = await ObtenerRevenue(WooCommerceStore.Montella, year, month);
        resumen.UnidadesMontella = await ObtenerUnidades(WooCommerceStore.Montella, year, month);
        resumen.TopProductosMontella = await ObtenerTopProductos(WooCommerceStore.Montella, year, month);

        resumen.TotalAlenka = await ObtenerRevenue(WooCommerceStore.Alenka, year, month);
        resumen.UnidadesAlenka = await ObtenerUnidades(WooCommerceStore.Alenka, year, month);
        resumen.TopProductosAlenka = await ObtenerTopProductos(WooCommerceStore.Alenka, year, month);

        return resumen;
    }

    private async Task<decimal> ObtenerRevenue(WooCommerceStore tienda, int year, int month)
    {
        var stats = await _wooService.GetMonthlyStatsSimpleAsync(year, month, tienda);
        return stats?.TotalRevenue ?? 0;
    }

    private async Task<int> ObtenerUnidades(WooCommerceStore tienda, int year, int month)
    {
        var stats = await _wooService.GetMonthlyStatsSimpleAsync(year, month, tienda);
        return stats?.UnitsSold ?? 0;
    }

    private async Task<List<string>> ObtenerTopProductos(WooCommerceStore tienda, int year, int month)
    {
        var top = await _wooService.GetTopProductsAsync(
            new DateTime(year, month, 1),
            new DateTime(year, month, DateTime.DaysInMonth(year, month)),
            tienda
        );

        return top.Take(3).Select(p => p.ProductName).ToList();
    }
}
