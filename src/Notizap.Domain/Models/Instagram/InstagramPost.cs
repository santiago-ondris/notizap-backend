public class InstagramPost
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string PostId { get; set; } = default!;
    public string Cuenta { get; set; } = default!;
    public DateTime FechaPublicacion { get; set; }

    public string Url { get; set; } = default!;
    public string ImageUrl { get; set; } = default!;
    public string? Content { get; set; }

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

    public string BusinessId { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
