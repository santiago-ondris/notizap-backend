public class PostMetricDto
{
    public string PostId { get; set; } = default!;
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
    public int Shares { get; set; }
    public long Interactions { get; set; }
    public double Engagement { get; set; }
    public int Impressions { get; set; }
    public int Reach { get; set; }
    public int Saved { get; set; }
    public int VideoViews { get; set; }

    public double Clicks { get; set; }
}
