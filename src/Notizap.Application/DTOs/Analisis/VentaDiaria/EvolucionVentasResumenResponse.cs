public class EvolucionVentasResumenResponse
{
    public List<string> Fechas { get; set; } = new();
    public EvolucionDatasetDto Cantidad { get; set; } = new();
    public EvolucionDatasetDto Facturacion { get; set; } = new();
}