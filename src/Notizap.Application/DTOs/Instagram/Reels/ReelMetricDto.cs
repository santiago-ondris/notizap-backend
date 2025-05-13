public class ReelMetricDto
{
    public string ReelId { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string BusinessId { get; set; } = default!;
    public string Type { get; set; } = default!;
    public PublishedAtDto PublishedAt { get; set; } = default!;
    public string Filter { get; set; } = default!;
    public string Url { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string ImageUrl { get; set; } = default!;
    public int Likes { get; set; }
    public int Comments { get; set; }
    public long Interactions { get; set; }
    public double Engagement { get; set; }
    public int Impressions { get; set; }
    public int Views { get; set; }
    public int Reach { get; set; }
    public int Saved { get; set; }
    public int Shares { get; set; }
    public int VideoViews { get; set; }
    public int ImpressionsPaid { get; set; }
    public int ImpressionsTotal { get; set; }
    public int ReachPaid { get; set; }
    public int VideoViewsPaid { get; set; }
    public int VideoViewsTotal { get; set; }
    public int PostClicksPaid { get; set; }
    public int PostInteractionsPaid { get; set; }
    public double Spend { get; set; }
}
