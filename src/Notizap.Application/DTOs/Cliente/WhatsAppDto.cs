using System.ComponentModel.DataAnnotations;

// DTO para mostrar plantillas
public class PlantillaWhatsAppDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public bool Activa { get; set; }
}

// DTO para crear plantilla
public class CrearPlantillaWhatsAppDto
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El mensaje es requerido")]
    [StringLength(1000, ErrorMessage = "El mensaje no puede exceder 1000 caracteres")]
    public string Mensaje { get; set; } = string.Empty;
    
    [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
    public string? Descripcion { get; set; }
    
    [Required(ErrorMessage = "La categoría es requerida")]
    [StringLength(50, ErrorMessage = "La categoría no puede exceder 50 caracteres")]
    public string Categoria { get; set; } = "General";
}

// DTO para actualizar plantilla
public class ActualizarPlantillaWhatsAppDto
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El mensaje es requerido")]
    [StringLength(1000, ErrorMessage = "El mensaje no puede exceder 1000 caracteres")]
    public string Mensaje { get; set; } = string.Empty;
    
    [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
    public string? Descripcion { get; set; }
    
    [Required(ErrorMessage = "La categoría es requerida")]
    [StringLength(50, ErrorMessage = "La categoría no puede exceder 50 caracteres")]
    public string Categoria { get; set; } = string.Empty;
    
    public bool Activa { get; set; } = true;
}