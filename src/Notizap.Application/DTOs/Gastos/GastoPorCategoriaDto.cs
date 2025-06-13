public class GastoPorCategoriaDto
{
    public string Categoria { get; set; } = null!;
    public decimal TotalMonto { get; set; }
    public int CantidadGastos { get; set; }
    public decimal Porcentaje { get; set; }
}