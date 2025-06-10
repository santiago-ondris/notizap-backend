public class CompraConDetallesDto
{
    public DateTime Fecha { get; set; }
    public string? Canal { get; set; }
    public string? Sucursal { get; set; }
    public decimal Total { get; set; }
    public List<DetalleCompraDto> Detalles { get; set; } = new List<DetalleCompraDto>();
}

public class DetalleCompraDto
{
    public string? Producto { get; set; }
    public int Cantidad { get; set; }
    public decimal Total { get; set; }
    public string? Marca { get; set; }
    public string? Categoria { get; set; }
}
