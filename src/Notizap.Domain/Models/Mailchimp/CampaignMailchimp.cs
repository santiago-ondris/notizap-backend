public class CampaignMailchimp
{
    public int Id { get; set; }
    public string CampaignId { get; set; } = null!; // ID real de Mailchimp
    public string Title { get; set; } = null!;
    public DateTime SendTime { get; set; }
    public int EmailsSent { get; set; }
    public decimal OpenRate { get; set; }
    public decimal ClickRate { get; set; }
    public int Conversions { get; set; }

    public string Cuenta { get; set; } = null!; // Montella o Alenka
}
