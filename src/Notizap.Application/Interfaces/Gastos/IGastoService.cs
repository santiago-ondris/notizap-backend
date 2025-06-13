    public interface IGastoService
    {
        Task<IEnumerable<GastoDto>> ObtenerTodosAsync();
        Task<GastoDto?> ObtenerPorIdAsync(int id);
        Task<GastoDto> CrearAsync(CreateGastoDto dto);
        Task<GastoDto?> ActualizarAsync(int id, UpdateGastoDto dto);
        Task<bool> EliminarAsync(int id);

        // Nuevos métodos para funcionalidades avanzadas
        Task<(IEnumerable<GastoDto> gastos, int totalCount)> ObtenerConFiltrosAsync(GastoFiltrosDto filtros);
        Task<GastoResumenDto> ObtenerResumenMensualAsync(int año, int mes);
        Task<IEnumerable<GastoPorCategoriaDto>> ObtenerGastosPorCategoriaAsync(DateTime? desde = null, DateTime? hasta = null);
        Task<IEnumerable<string>> ObtenerCategoriasAsync();
        Task<IEnumerable<GastoTendenciaDto>> ObtenerTendenciaMensualAsync(int meses = 12);
        Task<IEnumerable<GastoDto>> ObtenerGastosRecurrentesAsync();
        Task<IEnumerable<GastoDto>> ObtenerTopGastosAsync(int cantidad = 5, DateTime? desde = null, DateTime? hasta = null);
    }