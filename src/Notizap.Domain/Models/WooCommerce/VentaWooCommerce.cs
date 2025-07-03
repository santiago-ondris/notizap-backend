using System.ComponentModel.DataAnnotations;
public class VentaWooCommerce
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Tienda { get; set; } = string.Empty;
    
    [Range(1, 12)]
    public int Mes { get; set; }
    
    [Range(2020, 2030)]
    public int Año { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal MontoFacturado { get; set; }
    
    [Range(0, int.MaxValue)]
    public int UnidadesVendidas { get; set; }
    
    public string TopProductos { get; set; } = "[]"; // JSON string
    
    public string TopCategorias { get; set; } = "[]"; // JSON string
    
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    
    public DateTime? FechaActualizacion { get; set; }
    
    // Propiedades calculadas para facilitar consultas
    public string PeriodoCompleto => $"{Mes:D2}/{Año}";
    
    public string TiendaNormalizada => Tienda.ToUpperInvariant();
}