using System.ComponentModel.DataAnnotations;
public class SucursalVenta
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public bool AbreSabadoTarde { get; set; } = true; // Por defecto sí abre

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Relaciones
    public virtual ICollection<VentaVendedora> Ventas { get; set; } = new List<VentaVendedora>();

    // Método helper para verificar si abre en sabado tarde
    public bool PuedeVenderSabadoTarde()
    {
        return AbreSabadoTarde;
    }

    // Método para obtener sucursales que no abren sabado tarde
    public static List<string> SucursalesConHorarioEspecial()
    {
        return new List<string> { "25 de mayo", "DEAN FUNES" };
    }
}