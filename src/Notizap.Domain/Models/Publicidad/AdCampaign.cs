public class AdCampaign
{
    public int Id { get; set; }
    public string CampaignId { get; set; } = null!;

    public int AdReportId { get; set; }
    public AdReport Reporte { get; set; } = null!;

    public string Nombre { get; set; } = null!;
    public string Tipo { get; set; } = null!;

    public decimal MontoInvertido { get; set; }

    public string Objetivo { get; set; } = null!;
    public string Resultados { get; set; } = null!;

    // métricas de la API
    public int Clicks { get; set; }
    public int Impressions { get; set; }
    public decimal Ctr { get; set; }
    public int Reach { get; set; }

    public int ValorResultado { get; set; }

    public int FollowersCount { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
}
