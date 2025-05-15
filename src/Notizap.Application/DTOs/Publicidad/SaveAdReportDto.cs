public class SaveAdReportDto
{
    public string UnidadNegocio { get; set; } = null!; // Montella, Alenka, Kids
    public string Plataforma { get; set; } = null!;    // Meta, Google
    public int Year { get; set; }
    public int Month { get; set; }
    public List<AdCampaignDto> Campañas { get; set; } = new();
}
