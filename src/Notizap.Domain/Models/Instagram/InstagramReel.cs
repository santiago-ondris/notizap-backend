public class InstagramReel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string ReelId { get; set; } = default!;
    public string Cuenta { get; set; } = default!;
    public DateTime FechaPublicacion { get; set; }

    public string Url { get; set; } = default!;
    public string ImageUrl { get; set; } = default!;
    public string Contenido { get; set; } = default!;

    public int Likes { get; set; }
    public int Comentarios { get; set; }
    public int Views { get; set; }
    public int Reach { get; set; }
    public double Engagement { get; set; }
    public int Interacciones { get; set; }
    public int VideoViews { get; set; }
    public int Guardados { get; set; }
    public int Compartidos { get; set; }

    public string BusinessId { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
