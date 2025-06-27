public class RendimientoLocalesFilterDto
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string SucursalNombre { get; set; } = string.Empty;
    public string? Turno { get; set; }
    public string? MetricaComparar { get; set; } = "monto"; // "monto" o "cantidad"
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 31; // Por defecto, un mes
    public bool ExcluirDomingos { get; set; } = true;

}
