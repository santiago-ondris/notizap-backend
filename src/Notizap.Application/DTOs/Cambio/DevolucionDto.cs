public class DevolucionDto : CreateDevolucionDto
{
    public int Id { get; set; }
    public bool LlegoAlDeposito { get; set; }
    public bool DineroDevuelto { get; set; }
    public bool NotaCreditoEmitida { get; set; }
}
