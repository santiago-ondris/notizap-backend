public class PublicidadResumenMensualDto
{
    public string UnidadNegocio { get; set; } = null!;
    public decimal MontoTotal { get; set; }
    public int TotalClicks { get; set; }
    public int TotalImpressions { get; set; }
    public int TotalReach { get; set; }
    public int TotalManualFollowers { get; set; }
    public int CampaignCount { get; set; }
}
