public class GastoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string Categoria { get; set; } = null!;
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
    public string? Proveedor { get; set; }
    public string? MetodoPago { get; set; }
    public bool EsRecurrente { get; set; }
    public string? FrecuenciaRecurrencia { get; set; }
    public bool EsImportante { get; set; }
    public DateTime FechaCreacion { get; set; }
}