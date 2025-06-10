using ClosedXML.Excel;

public class ExcelFinder
{
    public static int EncontrarFilaHeader(IXLWorksheet ws, string[] claves)
    {
        for (int row = 1; row <= 20; row++)
        {
            var rowCells = ws.Row(row).Cells().Select(c => c.GetString().ToUpper()).ToList();
            if (claves.All(clave => rowCells.Any(c => c.Contains(clave))))
                return row;
        }
        return -1;
    }
    public static int EncontrarColumna(IXLWorksheet ws, int headerRow, string nombreCol)
    {
        var cells = ws.Row(headerRow).Cells();
        foreach (var cell in cells)
        {
            if (cell.GetString().ToUpper().Contains(nombreCol.ToUpper()))
                return cell.Address.ColumnNumber;
        }
        throw new Exception($"No se encontró la columna '{nombreCol}' en el header.");
    }
    public static decimal ParsearMoneda(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return 0;
        var limpio = valor.Replace("$", "").Replace(" ", "").Replace(",", "");
        return decimal.TryParse(limpio, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : 0;
    }
} 