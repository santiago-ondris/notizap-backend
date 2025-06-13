public class Gasto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string Categoria { get; set; } = null!;
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow; 
    public string? Proveedor { get; set; } 
    public string? MetodoPago { get; set; }
    public bool EsRecurrente { get; set; } = false;
    public string? FrecuenciaRecurrencia { get; set; }
    public bool EsImportante { get; set; } = false;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}