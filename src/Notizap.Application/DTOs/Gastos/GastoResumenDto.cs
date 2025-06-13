public class GastoResumenDto
{
    public decimal TotalMes { get; set; }
    public decimal TotalMesAnterior { get; set; }
    public decimal PorcentajeCambio { get; set; }
    public string CategoriaMasGastada { get; set; } = null!;
    public decimal MontoCategoriaMasGastada { get; set; }
    public decimal PromedioMensual { get; set; }
    public int CantidadGastos { get; set; }
}