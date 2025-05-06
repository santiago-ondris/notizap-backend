public interface IMercadoLibreService
{
    Task<List<MercadoLibreManualReport>> GetAllAsync();
    Task<MercadoLibreManualReport?> GetByIdAsync(int id);
    Task<MercadoLibreManualReport> CreateAsync(MercadoLibreManualDto dto);
    Task<MercadoLibreManualReport?> UpdateAsync(int id, MercadoLibreManualDto dto);
    Task<bool> DeleteAsync(int id);
    Task<List<DailySalesDto>> GetSimulatedDailyStatsAsync(int year, int month);
}
