public class GastoTendenciaDto
{
    public int Año { get; set; }
    public int Mes { get; set; }
    public string MesNombre { get; set; } = null!;
    public decimal TotalMonto { get; set; }
    public int CantidadGastos { get; set; }
    public decimal PromedioGasto { get; set; }
}