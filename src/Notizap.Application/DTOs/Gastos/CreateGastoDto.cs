    public class CreateGastoDto
    {
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string Categoria { get; set; } = null!;
        public decimal Monto { get; set; }
    }