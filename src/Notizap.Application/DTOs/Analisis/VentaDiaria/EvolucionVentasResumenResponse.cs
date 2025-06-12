public class EvolucionVentasResumenResponse
{
    public List<string> Fechas { get; set; } = new();
    public List<EvolucionSucursalResumenDto> Sucursales { get; set; } = new();
}