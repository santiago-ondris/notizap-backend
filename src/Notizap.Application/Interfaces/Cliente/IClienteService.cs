public interface IClienteService
{
    Task<PagedResult<ClienteResumenDto>> GetAllAsync(int pageNumber = 1, int pageSize = 20);
    Task<ClienteDetalleDto?> GetByIdAsync(int id);
    Task<List<ClienteResumenDto>> GetRankingAsync(
        string ordenarPor = "montoTotal",
        int top = 10,
        DateTime? desde = null,
        DateTime? hasta = null,
        string? canal = null,
        string? sucursal = null,
        string? marca = null,
        string? categoria = null,
        bool modoExclusivoCanal = false,    
        bool modoExclusivoSucursal = false, 
        bool modoExclusivoMarca = false,     
        bool modoExclusivoCategoria = false);
    Task<List<ClienteResumenDto>> BuscarPorNombreAsync(string filtro);
    Task<PagedResult<ClienteResumenDto>> FiltrarAsync(
        DateTime? desde, 
        DateTime? hasta, 
        string? canal, 
        string? sucursal, 
        string? marca, 
        string? categoria, 
        bool modoExclusivoCanal,     
        bool modoExclusivoSucursal,  
        bool modoExclusivoMarca,     
        bool modoExclusivoCategoria,
        string ordenarPor = "montoTotal",
        int pageNumber = 1, 
        int pageSize = 12);
    Task MarcarInactivoAsync(int clienteId);
    Task<List<string>> GetCanalesDisponiblesAsync();
    Task<List<string>> GetSucursalesDisponiblesAsync();
    Task<List<string>> GetMarcasDisponiblesAsync();
    Task<List<string>> GetCategoriasDisponiblesAsync();
    Task ActualizarTelefonoAsync(int clienteId, string telefono);
    Task<byte[]> ExportToExcelAsync(
        DateTime? desde, 
        DateTime? hasta, 
        string? canal, 
        string? sucursal, 
        string? marca, 
        string? categoria,
        bool modoExclusivoCanal,    
        bool modoExclusivoSucursal, 
        bool modoExclusivoMarca,    
        bool modoExclusivoCategoria,
        string ordenarPor = "montoTotal");
}