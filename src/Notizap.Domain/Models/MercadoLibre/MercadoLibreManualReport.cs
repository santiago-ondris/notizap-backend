public class MercadoLibreManualReport
{
    public int Id { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public int UnitsSold { get; set; }

    public decimal Revenue { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
