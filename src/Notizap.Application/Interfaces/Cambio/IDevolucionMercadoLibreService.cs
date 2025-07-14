public interface IDevolucionMercadoLibreService
{
    // CRUD básico
    Task<IEnumerable<DevolucionMercadoLibreDto>> GetAllAsync();
    Task<DevolucionMercadoLibreDto?> GetByIdAsync(int id);
    Task<DevolucionMercadoLibreDto> CreateAsync(CreateDevolucionMercadoLibreDto dto);
    Task<DevolucionMercadoLibreDto?> UpdateAsync(int id, UpdateDevolucionMercadoLibreDto dto);
    Task<bool> DeleteAsync(int id);

    // Filtros y búsqueda
    Task<IEnumerable<DevolucionMercadoLibreDto>> GetFilteredAsync(DevolucionMercadoLibreFiltrosDto filtros);
    
    // Actualización específica de nota de crédito
    Task<bool> UpdateNotaCreditoAsync(int id, bool notaCreditoEmitida);
    Task<bool> UpdateTrasladoAsync(int id, bool trasladado);

    // Estadísticas
    Task<DevolucionMercadoLibreEstadisticasDto> GetEstadisticasAsync();
    Task<DevolucionMercadoLibreEstadisticasDto> GetEstadisticasFilteredAsync(DevolucionMercadoLibreFiltrosDto filtros);

    // Utilidades
    Task<IEnumerable<(int Año, int Mes, string NombreMes)>> GetMesesDisponiblesAsync();
    Task<bool> ExistsAsync(int id);
}