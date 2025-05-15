public class AdReportDto
{
    public int Id { get; set; }
    public string UnidadNegocio { get; set; } = null!;
    public string Plataforma { get; set; } = null!;
    public int Year { get; set; }
    public int Month { get; set; }
    public List<AdCampaignReadDto> Campañas { get; set; } = new();
}
