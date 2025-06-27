public class RendimientoLocalesResponseDto
{
    public List<RendimientoLocalesDiaDto> Dias { get; set; } = new();
    public List<RendimientoVendedoraResumenDto> ResumenVendedoras { get; set; } = new();
    public int TotalDias { get; set; }
    public int Pagina { get; set; }
    public int PageSize { get; set; }
    public int TotalPaginas { get; set; }
}
