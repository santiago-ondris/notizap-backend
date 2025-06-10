public class ClienteDetalleDto
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
    public List<CompraConDetallesDto>? Compras { get; set; }
    public List<TopProductoDto>? TopProductos { get; set; }
    public List<TopCategoriaDto>? TopCategorias { get; set; }
    public List<TopMarcaDto>? TopMarcas { get; set; }
}