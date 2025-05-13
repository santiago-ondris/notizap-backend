public class StoryMetricDto
{
    public string PostId { get; set; } = default!;
    public string BusinessId { get; set; } = default!;
    public string Type { get; set; } = default!;

    public PublishedAtDto PublishedAt { get; set; } = default!;

    public string Owner { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string ThumbnailUrl { get; set; } = default!;
    public string MediaUrl { get; set; } = default!;
    public string Permalink { get; set; } = default!;

    public int Impressions { get; set; }
    public int Reach { get; set; }
    public int Replies { get; set; }
    public int TapsForward { get; set; }
    public int TapsBack { get; set; }
    public int Exits { get; set; }

    public int ImpressionsPaid { get; set; }
    public int ImpressionsTotal { get; set; }
    public int ReachPaid { get; set; }
}
