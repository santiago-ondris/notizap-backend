public interface ICambioService
{
    Task<int> CrearCambioAsync(CreateCambioDto dto);
    Task<List<CambioDto>> ObtenerTodosAsync();
    Task<CambioDto?> ObtenerPorIdAsync(int id);
    Task<bool> ActualizarCambioAsync(int id, CambioDto dto);
    Task<bool> EliminarCambioAsync(int id);
}
