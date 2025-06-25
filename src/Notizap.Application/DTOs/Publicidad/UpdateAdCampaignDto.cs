public class UpdateAdCampaignDto
{
    public string CampaignId { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Tipo { get; set; } = null!;
    public decimal MontoInvertido { get; set; }
    public string Objetivo { get; set; } = null!;
    public string Resultados { get; set; } = null!;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int FollowersCount { get; set; }
    
    public int? Clicks { get; set; }
    public int? Impressions { get; set; }
    public decimal? Ctr { get; set; }
    public int? Reach { get; set; }
    public string? ValorResultado { get; set; }
}