public class Cambio
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Pedido { get; set; } = default!;
    public string Celular { get; set; } = default!;
    public string Nombre { get; set; } = default!;
    public string Apellido { get; set; } = default!;
    public string ModeloOriginal { get; set; } = default!;
    public string ModeloCambio { get; set; } = default!;
    public string Motivo { get; set; } = default!;
    public bool ParPedido { get; set; }
    public bool LlegoAlDeposito { get; set; }
    public bool YaEnviado { get; set; }
    public decimal? DiferenciaAbonada { get; set; }
    public decimal? DiferenciaAFavor { get; set; }
    public string? Envio { get; set; }
    public bool CambioRegistradoSistema { get; set; }
    public string? Observaciones { get; set; }
    public string? Email { get; set; }
}