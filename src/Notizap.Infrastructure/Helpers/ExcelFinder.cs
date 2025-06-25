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
    public static string? FormatearTelefono(string? telefonoOriginal)
    {
        if (string.IsNullOrWhiteSpace(telefonoOriginal))
            return null;

        // Limpiar el teléfono: quitar espacios, guiones, paréntesis, etc.
        var telefonoLimpio = new string(telefonoOriginal.Where(char.IsDigit).ToArray());
        
        if (string.IsNullOrEmpty(telefonoLimpio))
            return null;

        // Si ya empieza con 549, devolverlo tal como está
        if (telefonoLimpio.StartsWith("549"))
            return telefonoLimpio;
        
        // Si empieza con 54, agregar solo el 9
        if (telefonoLimpio.StartsWith("54"))
            return "549" + telefonoLimpio.Substring(2);
        
        // Si empieza con 9 (código de celular argentino), agregar 54
        if (telefonoLimpio.StartsWith("9"))
            return "54" + telefonoLimpio;
        
        // Si no empieza con 9, asumir que es un celular sin código y agregar 549
        return "549" + telefonoLimpio;
    }
} 