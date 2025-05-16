public class CreateDisplayAdDto : BasePublicidadDto
{
    public int VisitasConDisplay { get; set; }
    public int VisitasSinDisplay { get; set; }
    public int Clics { get; set; }
    public int Impresiones { get; set; }
    public int Alcance { get; set; }
    public decimal Inversion { get; set; }
    public decimal CostoPorVisita { get; set; }

    public List<DisplayAnuncioDto> Anuncios { get; set; } = new();
}