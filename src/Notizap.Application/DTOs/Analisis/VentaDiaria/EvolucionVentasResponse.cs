    public class EvolucionVentasResponse
    {
        public List<string> Fechas { get; set; } = new();
        public List<EvolucionProductoDto> Productos { get; set; } = new();
    }