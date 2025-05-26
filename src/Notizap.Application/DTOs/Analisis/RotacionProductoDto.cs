public class RotacionProductoDto
{
    public string? Producto { get; set; }
    public string? Color { get; set; }
    public string? PuntoDeVenta { get; set; } 
    public int CantidadComprada { get; set; }
    public int CantidadVendida { get; set; }
    public double TasaRotacion => CantidadComprada == 0 ? 0 : (double)CantidadVendida / CantidadComprada;
}
