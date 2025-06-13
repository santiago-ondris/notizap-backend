public class GastoFiltrosDto
{
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public string? Categoria { get; set; }
    public decimal? MontoMinimo { get; set; }
    public decimal? MontoMaximo { get; set; }
    public string? Busqueda { get; set; } 
    public bool? EsImportante { get; set; }
    public bool? EsRecurrente { get; set; }
    public string? OrdenarPor { get; set; } = "Fecha";
    public bool Descendente { get; set; } = true;
    public int Pagina { get; set; } = 1;
    public int TamañoPagina { get; set; } = 20;
}