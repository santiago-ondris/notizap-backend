using System.ComponentModel.DataAnnotations;

public class DevolucionMercadoLibre
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime Fecha { get; set; }

    [Required]
    [StringLength(200)]
    public string Cliente { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Pedido { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Modelo { get; set; } = string.Empty;

    public bool NotaCreditoEmitida { get; set; } = false;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }
}