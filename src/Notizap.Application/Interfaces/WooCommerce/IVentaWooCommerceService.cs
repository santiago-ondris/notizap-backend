public interface IVentaWooCommerceService
{
    // CRUD Básico
    Task<VentaWooCommerceDto> GetByIdAsync(int id);
    Task<IEnumerable<VentaWooCommerceDto>> GetAllAsync();
    Task<VentaWooCommerceDto> CreateAsync(CreateVentaWooCommerceDto createDto);
    Task<VentaWooCommerceDto> UpdateAsync(UpdateVentaWooCommerceDto updateDto);
    Task<bool> DeleteAsync(int id);

    // Consultas con filtros y paginación
    Task<(IEnumerable<VentaWooCommerceDto> Items, int TotalCount)> GetPagedAsync(VentaWooCommerceQueryDto queryDto);
    
    // Consultas específicas para el negocio
    Task<VentaWooCommerceDto?> GetByTiendaYPeriodoAsync(string tienda, int mes, int año);
    Task<IEnumerable<VentaWooCommerceDto>> GetByPeriodoAsync(int mes, int año);
    Task<IEnumerable<VentaWooCommerceDto>> GetByTiendaAsync(string tienda);
    Task<IEnumerable<VentaWooCommerceDto>> GetByAñoAsync(int año);

    // Dashboard y reportes (como tu Excel)
    Task<TotalesVentasDto> GetTotalesByPeriodoAsync(int mes, int año);
    Task<IEnumerable<ResumenVentasDto>> GetResumenByPeriodoAsync(int mes, int año);
    Task<IEnumerable<TotalesVentasDto>> GetTotalesByAñoAsync(int año);

    // Validaciones de negocio
    Task<bool> ExistsAsync(string tienda, int mes, int año);
    Task<bool> ExistsAsync(int id);

    // Estadísticas adicionales
    Task<decimal> GetTotalFacturadoByTiendaAsync(string tienda, int año);
    Task<int> GetTotalUnidadesByTiendaAsync(string tienda, int año);
    Task<IEnumerable<string>> GetTiendasDisponiblesAsync();
    Task<IEnumerable<int>> GetAñosDisponiblesAsync();

    // Operaciones de lote (útil para migraciones o cargas masivas)
    Task<IEnumerable<VentaWooCommerceDto>> CreateBatchAsync(IEnumerable<CreateVentaWooCommerceDto> createDtos);
    Task<bool> DeleteByPeriodoAsync(int mes, int año);
    Task<bool> DeleteByTiendaAsync(string tienda);

    // Métodos de comparación temporal
    Task<decimal> GetCrecimientoMensualAsync(string tienda, int mesActual, int añoActual, int mesAnterior, int añoAnterior);
    Task<IEnumerable<TotalesVentasDto>> GetEvolucionAnualAsync(string tienda, int año);
}