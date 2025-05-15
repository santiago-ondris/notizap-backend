public class AdReport
{
    public int Id { get; set; }

    public string UnidadNegocio { get; set; } = null!; // "Montella", "Alenka", "Kids"
    public string Plataforma { get; set; } = null!;     // "Meta", "Google"
    public int Year { get; set; }
    public int Month { get; set; }

    public List<AdCampaign> Campañas { get; set; } = new();
}