public interface IClienteService
{
    Task<PagedResult<ClienteResumenDto>> GetAllAsync(int pageNumber = 1, int pageSize = 20);
    Task<ClienteDetalleDto?> GetByIdAsync(int id);
        Task<List<ClienteResumenDto>> GetRankingAsync(
        string ordenarPor = "monto", 
        int top = 10,
        DateTime? desde = null,
        DateTime? hasta = null,
        string? canal = null,
        string? sucursal = null,
        string? marca = null,
        string? categoria = null);
    Task<List<ClienteResumenDto>> BuscarPorNombreAsync(string filtro);
    Task<PagedResult<ClienteResumenDto>> FiltrarAsync(
    DateTime? desde, DateTime? hasta, string? canal, string? sucursal, string? marca, string? categoria, int pageNumber = 1, int pageSize = 12);
    Task MarcarInactivoAsync(int clienteId);
    Task<List<string>> GetCanalesDisponiblesAsync();
    Task<List<string>> GetSucursalesDisponiblesAsync();
    Task<List<string>> GetMarcasDisponiblesAsync();
    Task<List<string>> GetCategoriasDisponiblesAsync();
}