    public class EvolucionSucursalDto
    {
        public string Sucursal { get; set; } = string.Empty;
        public List<int> Serie { get; set; } = new();
        public List<EvolucionVarianteDto> VariantesPorColor { get; set; } = new();
    }