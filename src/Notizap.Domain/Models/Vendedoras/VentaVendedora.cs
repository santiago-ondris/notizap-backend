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
        var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
        var fechaAr = TimeZoneInfo.ConvertTime(fecha, tz);
        var h = fechaAr.TimeOfDay;

        var inicioM = TimeSpan.FromHours(8);
        var finM    = TimeSpan.FromHours(14).Add(TimeSpan.FromMinutes(30));  // 14:30
        var finT    = TimeSpan.FromHours(22);                              // 22:00

        if (h >= inicioM && h <= finM)
            return TurnoVenta.Mañana;

        if (h >  finM && h <= finT)  
            return TurnoVenta.Tarde;

        return h < inicioM
            ? TurnoVenta.Mañana
            : TurnoVenta.Tarde;
    }

    public static bool EsProductoEspecial(string producto)
    {
        var palabrasEspeciales = new[] { "DESCUENTO", "CUPON", "CLUB", "GENERICO", "GIFT", "RESEÑA" };
        return palabrasEspeciales.Any(palabra => 
            producto.ToUpper().Contains(palabra.ToUpper()));
    }

    public static bool EsDomingo(DateTime fecha)
    {
        return fecha.DayOfWeek == DayOfWeek.Sunday;
    }
}