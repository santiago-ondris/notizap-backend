public class GastosResumenBuilder
{
    private readonly IGastoService _gastoService;

    public GastosResumenBuilder(IGastoService gastoService)
    {
        _gastoService = gastoService;
    }

    public async Task<GastosResumenDto> ConstruirAsync(int year, int month)
    {
        var gastos = await _gastoService.ObtenerTodosAsync();

        var gastosDelMes = gastos
            .Where(g => g.Fecha.Year == year && g.Fecha.Month == month)
            .ToList();

        var total = gastosDelMes.Sum(g => g.Monto);

        var porCategoria = gastosDelMes
            .GroupBy(g => g.Categoria)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Monto));

        return new GastosResumenDto
        {
            TotalGastos = total,
            PorCategoria = porCategoria
        };
    }
}
