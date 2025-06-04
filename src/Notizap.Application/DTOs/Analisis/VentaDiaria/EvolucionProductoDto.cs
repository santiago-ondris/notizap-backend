    public class EvolucionProductoDto
    {
        public string Nombre { get; set; } = string.Empty; 
        public List<EvolucionSucursalDto> Sucursales { get; set; } = new();
        public List<EvolucionVarianteDto> VariantesPorColor { get; set; } = new();
    }

    public class EvolucionVarianteDto
{
    public string Color { get; set; } = string.Empty;
    public List<int> Serie { get; set; } = new();
}