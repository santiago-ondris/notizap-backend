public class InstagramStory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string PostId { get; set; } = default!;
    public string Cuenta { get; set; } = default!;
    public DateTime FechaPublicacion { get; set; }

    public string? MediaUrl { get; set; }
    public string ThumbnailUrl { get; set; } = default!;
    public string Permalink { get; set; } = default!;
    public string? Content { get; set; }

    public int Impressions { get; set; }
    public int Reach { get; set; }
    public int Replies { get; set; }
    public int TapsForward { get; set; }
    public int TapsBack { get; set; }
    public int Exits { get; set; }

    public string BusinessId { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
