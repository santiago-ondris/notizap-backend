public class CreateDevolucionDto
{
    public DateTime Fecha { get; set; }
    public string Pedido { get; set; } = null!;
    public string Celular { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public string Motivo { get; set; } = null!;
    public decimal? Monto { get; set; }
    public decimal? PagoEnvio { get; set; }
    public string Responsable { get; set; } = null!;
}
