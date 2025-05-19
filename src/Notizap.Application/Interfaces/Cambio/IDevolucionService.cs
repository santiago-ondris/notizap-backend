public interface IDevolucionService
{
    Task<int> CrearDevolucionAsync(CreateDevolucionDto dto);
    Task<List<DevolucionDto>> ObtenerTodasAsync();
    Task<DevolucionDto?> ObtenerPorIdAsync(int id);
    Task<bool> ActualizarDevolucionAsync(int id, DevolucionDto dto);
    Task<bool> EliminarDevolucionAsync(int id);
}