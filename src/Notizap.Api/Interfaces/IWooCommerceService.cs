public interface IWooCommerceService
{
    Task<SalesStatsDto> GetMonthlyStatsSimpleAsync(int year, int month, WooCommerceStore store);
    Task<SalesStatsDto> GetStatsByRangeAsync(DateTime from, DateTime to, WooCommerceStore store);
    Task<List<ProductStatsDto>> GetTopProductsAsync(DateTime from, DateTime to, WooCommerceStore store, int topN = 5);
    Task<WooCommerceMonthlyReport> SaveMonthlyReportAsync(SaveWooMonthlyReportDto dto);
    Task<WooCommerceMonthlyReport?> GetSavedMonthlyReportAsync(int year, int month, WooCommerceStore store);
}