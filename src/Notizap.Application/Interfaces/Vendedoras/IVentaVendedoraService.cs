public interface IVentaVendedoraService
{
    // Upload y gestión de archivos
    Task<(bool Success, string Message, VentaVendedoraStatsDto? Stats)> SubirArchivoVentasAsync(
        Stream archivoStream, bool sobreescribirDuplicados = false);

    Task<bool> EliminarVentasPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);

    // Consultas principales
    Task<(List<VentaVendedoraDto> Ventas, int TotalRegistros)> ObtenerVentasAsync(
        VentaVendedoraFilterDto filtros);

    Task<VentaVendedoraStatsDto> ObtenerEstadisticasAsync(VentaVendedoraFilterDto filtros);

    // Datos maestros
    Task<List<string>> ObtenerSucursalesAsync();

    Task<List<string>> ObtenerVendedoresAsync();

    Task<(DateTime? FechaMinima, DateTime? FechaMaxima)> ObtenerRangoFechasAsync();

    Task<(DateTime? FechaMinima, DateTime? FechaMaxima)> ObtenerUltimaSemanaConDatosAsync();

    // Análisis específicos
    Task<List<VentaPorDiaDto>> ObtenerVentasPorDiaAsync(VentaVendedoraFilterDto filtros);

    Task<List<VentaPorVendedoraDto>> ObtenerTopVendedorasAsync(VentaVendedoraFilterDto filtros, int top = 10);

    Task<List<VentaPorSucursalDto>> ObtenerVentasPorSucursalAsync(VentaVendedoraFilterDto filtros);

    Task<List<VentaPorTurnoDto>> ObtenerVentasPorTurnoAsync(VentaVendedoraFilterDto filtros);

    // Validaciones y utilidades
    Task<bool> ExistenDatosEnRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);

    Task<List<string>> ValidarArchivoSinProcesarAsync(Stream archivoStream);
    Task<List<VentaPorVendedoraDto>> ObtenerTodasLasVendedorasAsync(VentaVendedoraFilterDto filtros);

}