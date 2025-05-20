public class EnviosResumenBuilder
{
    private readonly IEnvioService _envioService;

    public EnviosResumenBuilder(IEnvioService envioService)
    {
        _envioService = envioService;
    }

    public async Task<EnviosResumenDto> ConstruirAsync(int year, int month)
    {
        var resumen = await _envioService.ObtenerResumenMensualAsync(year, month);

        var porTipo = new Dictionary<string, int>
        {
            ["OCA"] = resumen.TotalOca,
            ["Andreani"] = resumen.TotalAndreani,
            ["Retiros en Sucursal"] = resumen.TotalRetirosSucursal,
            ["Córdoba - Roberto"] = resumen.TotalRoberto,
            ["Córdoba - Tino"] = resumen.TotalTino,
            ["Córdoba - Caddy"] = resumen.TotalCaddy,
            ["Mercado Libre"] = resumen.TotalMercadoLibre
        };

        var total = 0;
        foreach (var cantidad in porTipo.Values)
            total += cantidad;

        return new EnviosResumenDto
        {
            PorTipo = porTipo,
            TotalEnvios = total
        };
    }
}
