public class InstagramSeguidores
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Cuenta { get; set; } = default!;
    public DateTime Date { get; set; }
    public int Value { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}