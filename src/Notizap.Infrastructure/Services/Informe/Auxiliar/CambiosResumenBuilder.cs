public class CambiosResumenBuilder
{
    private readonly ICambioService _cambioService;
    private readonly IDevolucionService _devolucionService;

    public CambiosResumenBuilder(
        ICambioService cambioService,
        IDevolucionService devolucionService)
    {
        _cambioService = cambioService;
        _devolucionService = devolucionService;
    }

    public async Task<CambiosResumenDto> ConstruirAsync(int year, int month)
    {
        var cambios = await _cambioService.ObtenerTodosAsync();
        var devoluciones = await _devolucionService.ObtenerTodasAsync();

        var cantCambios = cambios.Count(c => c.Fecha.Year == year && c.Fecha.Month == month);
        var cantDevoluciones = devoluciones.Count(d => d.Fecha.Year == year && d.Fecha.Month == month);

        return new CambiosResumenDto
        {
            CantidadCambios = cantCambios,
            CantidadDevoluciones = cantDevoluciones
        };
    }
}
