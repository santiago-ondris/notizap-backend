public class ExcelTopProductoML
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public string ModeloColor { get; set; } = null!;
    public int Cantidad { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
}
