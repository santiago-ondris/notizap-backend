public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public int CantidadCompras { get; set; }
    public decimal MontoTotalGastado { get; set; }
    public DateTime FechaPrimeraCompra { get; set; }
    public DateTime FechaUltimaCompra { get; set; }
    public ICollection<Compra> Compras { get; set; } = new List<Compra>(); 
    public string? Canales { get; set; } 
    public string? Sucursales { get; set; } 
    public string? Observaciones { get; set; } 
}