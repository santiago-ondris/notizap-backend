public class MercadoLibreResumenBuilder
{
    private readonly IMercadoLibreService _mlService;
    private readonly IMercadoLibrePublicidadService _adsService;
    private readonly IMercadoLibreExcelProcessor _excelProcessor;

    public MercadoLibreResumenBuilder(
        IMercadoLibreService mlService,
        IMercadoLibrePublicidadService adsService,
        IMercadoLibreExcelProcessor excelProcessor)
    {
        _mlService = mlService;
        _adsService = adsService;
        _excelProcessor = excelProcessor;
    }

    public async Task<MercadoLibreResumenDto> ConstruirAsync(int year, int month)
    {
        // Ventas
        var reporte = (await _mlService.GetAllAsync())
            .FirstOrDefault(r => r.Year == year && r.Month == month);

        // Top productos
        var top = await _excelProcessor.ObtenerAnalisisPorMesAsync(year, month);

        // Publicidad por tipo
        var productAds = await _adsService.ObtenerPorTipoYMesAsync(TipoPublicidadML.ProductAds, year, month);
        var brandAds = await _adsService.ObtenerPorTipoYMesAsync(TipoPublicidadML.BrandAds, year, month);
        var displayAds = await _adsService.ObtenerPorTipoYMesAsync(TipoPublicidadML.DisplayAds, year, month);

        var resumen = new MercadoLibreResumenDto
        {
            Total = reporte?.Revenue ?? 0,
            Unidades = reporte?.UnitsSold ?? 0,
            TopProductos = top.Take(3).Select(p => p.ModeloColor).ToList(),

            InversionProductAds = productAds.Sum(r => r.Inversion),
            InversionBrandAds = brandAds.Sum(r => r.Inversion),
            InversionDisplayAds = displayAds.Sum(r => r.Inversion),
            InversionTotal = productAds.Sum(r => r.Inversion) +
                             brandAds.Sum(r => r.Inversion) +
                             displayAds.Sum(r => r.Inversion),

            CampañasProductAds = productAds.Count,
            CampañasBrandAds = brandAds.Count,
            CampañasDisplayAds = displayAds.Count
        };

        return resumen;
    }
}
