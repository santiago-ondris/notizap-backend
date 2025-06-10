public class Compra
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public DateTime Fecha { get; set; } 
    public string? Canal { get; set; } 
    public string? Sucursal { get; set; }
    public decimal Total { get; set; } // TOTAL
    public ICollection<CompraDetalle>? Detalles { get; set; }
}