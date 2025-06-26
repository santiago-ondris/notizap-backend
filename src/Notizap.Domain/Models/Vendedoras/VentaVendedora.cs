using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class VentaVendedora
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int SucursalId { get; set; }

    [Required]
    public int VendedorId { get; set; }

    [Required]
    [StringLength(200)]
    public string Producto { get; set; } = string.Empty;

    [Required]
    public DateTime Fecha { get; set; }

    [Required]
    public int Cantidad { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    [Required]
    public TurnoVenta Turno { get; set; }

    // Campo para marcar si es un producto especial (descuento)
    public bool EsProductoDescuento { get; set; } = false;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Relaciones
    public virtual SucursalVenta Sucursal { get; set; } = null!;
    public virtual VendedorVenta Vendedor { get; set; } = null!;

    // Métodos helper
    public bool EsVentaValida()
    {
        // Si es producto de descuento y cantidad es -1, no contar para cantidad pero sí para total
        return !EsProductoDescuento || Cantidad != -1;
    }

    public int GetCantidadReal()
    {
        // Si es producto descuento con cantidad -1, retornar 0 para estadísticas de cantidad
        if (EsProductoDescuento && Cantidad == -1)
            return 0;
        
        return Cantidad;
    }

    public static TurnoVenta DeterminarTurno(DateTime fecha)
    {
        var hora = fecha.TimeOfDay;
        
        // Mañana: 8:00 - 14:30
        if (hora >= new TimeSpan(8, 0, 0) && hora <= new TimeSpan(14, 30, 0))
            return TurnoVenta.Mañana;
        
        // Tarde: 15:00 - 22:00
        if (hora >= new TimeSpan(15, 0, 0) && hora <= new TimeSpan(22, 0, 0))
            return TurnoVenta.Tarde;

        // Por defecto mañana (casos edge)
        return TurnoVenta.Mañana;
    }

    public static bool EsProductoEspecial(string producto)
    {
        var palabrasEspeciales = new[] { "DESCUENTO", "CUPON", "CLUB", "GENERICO", "GIFT" };
        return palabrasEspeciales.Any(palabra => 
            producto.ToUpper().Contains(palabra.ToUpper()));
    }

    public static bool EsDomingo(DateTime fecha)
    {
        return fecha.DayOfWeek == DayOfWeek.Sunday;
    }
}