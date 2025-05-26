public class EvolucionStockPorPuntoDeVentaDto
{
    public string? PuntoDeVenta { get; set; }
    public List<EvolucionStockDiaDto>? Evolucion { get; set; }
}

public class EvolucionStockDiaDto
{
    public DateTime Fecha { get; set; }
    public int Stock { get; set; }
}
