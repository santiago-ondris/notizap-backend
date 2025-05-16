using Microsoft.AspNetCore.Http;

public interface IMercadoLibreExcelProcessor
{
    Task<List<TopProductoColorDto>> ObtenerTopProductosPorColorAsync(IFormFile archivo, int top = 10);
    Task GuardarAnalisisAsync(SaveExcelTopDto dto);
    Task<List<ExcelTopProductoDto>> ObtenerAnalisisPorMesAsync(int year, int month);
    Task<List<ExcelTopProductoDto>> ObtenerHistoricoAsync();
}
