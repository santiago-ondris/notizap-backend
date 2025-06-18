public class CreateCambioSimpleDto
{
    public DateTime Fecha { get; set; }
    public string Pedido { get; set; } = null!;
    public string Celular { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Apellido { get; set; }
    public string? Email { get; set; }
    public string ModeloOriginal { get; set; } = null!;
    public string ModeloCambio { get; set; } = null!;
    public string Motivo { get; set; } = null!;
    public bool ParPedido { get; set; }
    public decimal? DiferenciaAbonada { get; set; }
    public decimal? DiferenciaAFavor { get; set; }
    public string? Responsable { get; set; }
    public string? Observaciones { get; set; }
}

public class CambioSimpleDto : CreateCambioSimpleDto
{
    public int Id { get; set; }
    public bool LlegoAlDeposito { get; set; }
    public bool YaEnviado { get; set; }
    public bool CambioRegistradoSistema { get; set; }
}