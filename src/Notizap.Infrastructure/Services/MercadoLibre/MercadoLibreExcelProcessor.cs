using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

public class MercadoLibreExcelProcessor : IMercadoLibreExcelProcessor
{
    private readonly NotizapDbContext _context;
    public MercadoLibreExcelProcessor(NotizapDbContext context)
    {
        _context = context;
    }
    public async Task<List<TopProductoColorDto>> ObtenerTopProductosPorColorAsync(IFormFile archivo, int top = 10)
    {
        var conteo = new Dictionary<string, int>();

        using var stream = new MemoryStream();
        await archivo.CopyToAsync(stream);
        using var workbook = new XLWorkbook(stream);
        var hoja = workbook.Worksheet(1);

        foreach (var fila in hoja.RowsUsed().Skip(1)) // Saltar encabezado
        {
            var titulo = fila.Cell(1).GetString().Trim();
            var variante = fila.Cell(2).GetString().Trim();

            if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(variante))
                continue;

            var match = Regex.Match(variante, @"Color\s*:\s*([^|]+)", RegexOptions.IgnoreCase);
            if (!match.Success) continue;

            var color = match.Groups[1].Value.Trim();
            var clave = $"{titulo} color {color}";

            if (conteo.ContainsKey(clave))
                conteo[clave]++;
            else
                conteo[clave] = 1;
        }

        return conteo
            .OrderByDescending(x => x.Value)
            .Take(top)
            .Select(x => new TopProductoColorDto
            {
                ModeloColor = x.Key,
                Cantidad = x.Value
            })
            .ToList();
    }

    public async Task GuardarAnalisisAsync(SaveExcelTopDto dto)
    {
        var conteo = await ObtenerTopProductosPorColorAsync(dto.Archivo, top: int.MaxValue);

        var entidades = conteo.Select(x => new ExcelTopProductoML
        {
            Year = dto.Year,
            Month = dto.Month,
            ModeloColor = x.ModeloColor,
            Cantidad = x.Cantidad,
            FechaCreacionUtc = DateTime.UtcNow
        }).ToList();

        _context.ExcelTopProductosML.AddRange(entidades);
        await _context.SaveChangesAsync();
    }
    public async Task<List<ExcelTopProductoDto>> ObtenerAnalisisPorMesAsync(int year, int month)
    {
        return await _context.ExcelTopProductosML
            .Where(x => x.Year == year && x.Month == month)
            .OrderByDescending(x => x.Cantidad)
            .Select(x => new ExcelTopProductoDto
            {
                ModeloColor = x.ModeloColor,
                Cantidad = x.Cantidad
            })
            .ToListAsync();
    }

    public async Task<List<ExcelTopProductoDto>> ObtenerHistoricoAsync()
    {
        return await _context.ExcelTopProductosML
            .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
            .Select(x => new ExcelTopProductoDto
            {
                ModeloColor = x.ModeloColor,
                Cantidad = x.Cantidad
            })
            .ToListAsync();
    }
}
