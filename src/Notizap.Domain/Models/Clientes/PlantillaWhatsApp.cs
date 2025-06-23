using System.ComponentModel.DataAnnotations;

public class PlantillaWhatsApp
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Mensaje { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Descripcion { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Categoria { get; set; } = "General"; // General, Seguimiento, Oferta, Consulta, etc.
    
    [Required]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    
    [Required]
    [MaxLength(100)]
    public string CreadoPor { get; set; } = string.Empty; // Username del usuario que creó la plantilla
    
    public bool Activa { get; set; } = true;
}