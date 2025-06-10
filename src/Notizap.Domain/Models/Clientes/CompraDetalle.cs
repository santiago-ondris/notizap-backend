public class CompraDetalle
{
    public int Id { get; set; }
    public int CompraId { get; set; }
    public Compra? Compra { get; set; }
    public string? Producto { get; set; }
    public string? Marca { get; set; }
    public string? Categoria { get; set; }
    public int Cantidad { get; set; }
    public decimal Total { get; set; }
}