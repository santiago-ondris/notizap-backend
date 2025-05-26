public class CompraDetalleConFechaDto
{
    public string? Nro { get; set; }
    public string? Fecha { get; set; }
    public string? Proveedor { get; set; }
    public string? Producto { get; set; }
    public int Cantidad { get; set; }
    public decimal Total { get; set; }
    public string? Color { get; set; }
    public object? PuntoDeVenta { get; set; }
}