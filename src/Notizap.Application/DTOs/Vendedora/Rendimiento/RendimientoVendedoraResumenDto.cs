public class RendimientoVendedoraResumenDto
{
    public string VendedoraNombre { get; set; } = string.Empty;
    public int DiasTrabajados { get; set; }
    public int DiasCumplioMonto { get; set; }
    public int DiasCumplioCantidad { get; set; }
    public decimal PorcentajeCumplimientoMonto { get; set; }
    public decimal PorcentajeCumplimientoCantidad { get; set; }
}
