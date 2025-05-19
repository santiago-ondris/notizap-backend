public class Devolucion
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Pedido { get; set; } = default!;
    public string Celular { get; set; } = default!;
    public string Modelo { get; set; } = default!;
    public string Motivo { get; set; } = default!;
    public bool LlegoAlDeposito { get; set; }
    public bool DineroDevuelto { get; set; }
    public bool NotaCreditoEmitida { get; set; }
    public decimal? Monto { get; set; }
    public decimal? PagoEnvio { get; set; }

    // Metadatos
    public string Responsable { get; set; } = default!;
}
