public class WooResumenDto
{
    public decimal TotalMontella { get; set; }
    public int UnidadesMontella { get; set; }
    public List<string> TopProductosMontella { get; set; } = null!;

    public decimal TotalAlenka { get; set; }
    public int UnidadesAlenka { get; set; }
    public List<string> TopProductosAlenka { get; set; } = null!;
}