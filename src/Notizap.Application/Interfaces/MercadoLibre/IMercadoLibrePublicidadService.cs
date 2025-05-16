public interface IMercadoLibrePublicidadService
{
    Task<int> CrearProductAdAsync(CreateProductAdDto dto);
    Task<int> CrearBrandAdAsync(CreateBrandAdDto dto);
    Task<int> CrearDisplayAdAsync(CreateDisplayAdDto dto);

    Task<List<ReadAdDto>> ObtenerTodosAsync();
    Task<List<ReadAdDto>> ObtenerPorMesAsync(int year, int month);
    Task<ReadAdDto?> ObtenerPorIdAsync(int id);
    Task<decimal> ObtenerInversionTotalPorMesAsync(int year, int month);

    Task<bool> EliminarAsync(int id);

    Task<List<ReportePublicidadML>> ObtenerPorTipoYMesAsync(TipoPublicidadML tipo, int year, int month);
}
