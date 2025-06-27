public class RendimientoLocalesDiaDto
{
    public string SucursalNombre { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Turno { get; set; } = string.Empty; // "Mañana" o "Tarde"
    public decimal MontoTotal { get; set; }
    public int CantidadTotal { get; set; }
    public decimal PromedioMontoVendedora { get; set; }
    public decimal PromedioCantidadVendedora { get; set; }
    public List<RendimientoVendedoraDiaDto> Vendedoras { get; set; } = new();
}
