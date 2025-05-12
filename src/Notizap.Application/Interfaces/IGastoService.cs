    public interface IGastoService
    {
        Task<IEnumerable<GastoDto>> ObtenerTodosAsync();
        Task<GastoDto?> ObtenerPorIdAsync(int id);
        Task<GastoDto> CrearAsync(CreateGastoDto dto);
        Task<GastoDto?> ActualizarAsync(int id, UpdateGastoDto dto);
        Task<bool> EliminarAsync(int id);
    }