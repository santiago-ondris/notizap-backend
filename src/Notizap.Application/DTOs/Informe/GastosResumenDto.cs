public class GastosResumenDto
{
    public decimal TotalGastos { get; set; }
    public Dictionary<string, decimal>? PorCategoria { get; set; }
}