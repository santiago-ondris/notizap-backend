public interface ICambioService
{
    Task<int> CrearCambioAsync(CreateCambioSimpleDto dto);
    Task<List<CambioSimpleDto>> ObtenerTodosAsync();
    Task<CambioSimpleDto?> ObtenerPorIdAsync(int id);
    Task<bool> ActualizarCambioAsync(int id, CambioSimpleDto dto);
    Task<bool> ActualizarEstadosAsync(int id, bool llegoAlDeposito, bool yaEnviado, bool cambioRegistradoSistema, bool parPedido);
    Task<bool> EliminarCambioAsync(int id);
}