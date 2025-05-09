public class MailchimpStatsDto
{
  public string CampaignTitle { get; set; } = default!;
  public DateTime SendTime { get; set; }
  public int EmailsSent { get; set; }

  public decimal OpenRate { get; set; }
  public decimal ClickRate { get; set; }
  public int Conversions { get; set; }
}
public class MailchimpCampaignDto
{
  public string Id { get; set; } = default!;
  public string Title { get; set; } = default!;
  public DateTime SendTime { get; set; }
}
public class MailchimpHighlightsDto
{
  public string BestOpenRateCampaign { get; set; } = default!;
  public decimal BestOpenRate { get; set; }

  public string BestClickRateCampaign { get; set; } = default!;
  public decimal BestClickRate { get; set; }

  public string BestConversionCampaign { get; set; } = default!;
  public int BestConversions { get; set; }
}