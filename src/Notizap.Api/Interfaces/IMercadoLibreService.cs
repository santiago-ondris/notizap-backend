public interface IMercadoLibreService
{
    Task<SalesStatsDto> GetStatsByRangeAsync(DateTime from, DateTime to);
    Task<List<ProductStatsDto>> GetTopProductsAsync(DateTime from, DateTime to, int topN = 5);
}