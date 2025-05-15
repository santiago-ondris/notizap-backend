public interface IAdService
{
    Task<List<AdReportDto>> GetAllAsync();
    Task<AdReportDto?> GetByIdAsync(int id);
    Task<AdReportDto> CreateAsync(SaveAdReportDto dto);
    Task<AdReportDto?> UpdateAsync(int id, SaveAdReportDto dto);
    Task<bool> DeleteAsync(int id);
    Task<List<PublicidadResumenMensualDto>> GetResumenMensualAsync(int year, int month);
    Task<SyncResultDto> SyncReportFromApiAsync(int reportId, string adAccountId, DateTime from, DateTime to);
}