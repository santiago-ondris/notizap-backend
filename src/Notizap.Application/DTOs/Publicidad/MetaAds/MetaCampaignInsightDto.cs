public class MetaCampaignInsightDto
{
    public string CampaignId { get; set; } = null!;
    public string CampaignName { get; set; } = null!;
    public string Objective { get; set; } = null!;
    public decimal Spend { get; set; }
    public int Clicks { get; set; }
    public int Impressions { get; set; }
    public decimal Ctr { get; set; }
    public int Reach { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public string ResultadoPrincipal { get; set; } = null!;
    public int ValorResultado { get; set; }
}
