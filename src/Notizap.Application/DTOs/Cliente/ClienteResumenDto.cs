public class ClienteResumenDto
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public int CantidadCompras { get; set; }
    public decimal MontoTotalGastado { get; set; }
    public DateTime FechaPrimeraCompra { get; set; }
    public DateTime FechaUltimaCompra { get; set; }
    public string? Canales { get; set; } 
    public string? Sucursales { get; set; } 
    public string? Observaciones { get; set; }
    public string? Telefono { get; set; }
}