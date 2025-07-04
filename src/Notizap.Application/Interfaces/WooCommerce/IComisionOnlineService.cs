public interface IComisionOnlineService
{
    // CRUD Básico
    Task<ComisionOnlineDto> GetByIdAsync(int id);
    Task<IEnumerable<ComisionOnlineDto>> GetAllAsync();
    Task<ComisionOnlineDto> CreateAsync(CreateComisionOnlineDto createDto);
    Task<ComisionOnlineDto> UpdateAsync(UpdateComisionOnlineDto updateDto);
    Task<bool> DeleteAsync(int id);

    // Consultas con filtros y paginación
    Task<(IEnumerable<ComisionOnlineDto> Items, int TotalCount)> GetPagedAsync(ComisionOnlineQueryDto queryDto);

    // Consultas específicas del negocio
    Task<ComisionOnlineDto?> GetByPeriodoAsync(int mes, int año);
    Task<IEnumerable<ComisionOnlineDto>> GetByAñoAsync(int año);

    // Validaciones
    Task<bool> ExistsAsync(int mes, int año);
    Task<bool> ExistsAsync(int id);

    // Cálculos
    CalculoComisionDto CalcularComision(int mes, int año, decimal totalSinNC, decimal montoAndreani, decimal montoOCA, decimal montoCaddy);
    
    // Operaciones de lote
    Task<bool> DeleteByPeriodoAsync(int mes, int año);
    Task<IEnumerable<int>> GetAñosDisponiblesAsync();
}