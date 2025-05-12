public class Gasto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string Categoria { get; set; } = null!;
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }= DateTime.UtcNow;
}