using System.ComponentModel.DataAnnotations;
public class VendedorVenta
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Email { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Relaciones
    public virtual ICollection<VentaVendedora> Ventas { get; set; } = new List<VentaVendedora>();

    // Métodos helper para análisis
    public decimal GetVentasTotalPeriodo(DateTime fechaInicio, DateTime fechaFin)
    {
        return Ventas
            .Where(v => v.Fecha >= fechaInicio && v.Fecha <= fechaFin)
            .Sum(v => v.Total);
    }

    public int GetCantidadVentasPeriodo(DateTime fechaInicio, DateTime fechaFin)
    {
        return Ventas
            .Where(v => v.Fecha >= fechaInicio && v.Fecha <= fechaFin)
            .Sum(v => v.Cantidad);
    }

    public decimal GetPromedioVentasDiarias(DateTime fechaInicio, DateTime fechaFin)
    {
        var diasConVentas = Ventas
            .Where(v => v.Fecha >= fechaInicio && v.Fecha <= fechaFin)
            .Where(v => v.Fecha.DayOfWeek != DayOfWeek.Sunday) // Excluir domingos
            .GroupBy(v => v.Fecha.Date)
            .Count();

        if (diasConVentas == 0) return 0;

        var totalVentas = GetVentasTotalPeriodo(fechaInicio, fechaFin);
        return totalVentas / diasConVentas;
    }
}