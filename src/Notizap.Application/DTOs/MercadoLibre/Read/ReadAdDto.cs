public class ReadAdDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = null!;
    public string NombreCampania { get; set; } = null!;
    public decimal Inversion { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }

    public object? Detalles { get; set; }
}