public class RendimientoVendedoraDiaDto
{
    public string VendedoraNombre { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public int Cantidad { get; set; }
    public bool CumplioMontoPromedio { get; set; }
    public bool CumplioCantidadPromedio { get; set; }
}
