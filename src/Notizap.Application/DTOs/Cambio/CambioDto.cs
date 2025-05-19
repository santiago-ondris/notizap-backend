public class CambioDto : CreateCambioDto
{
    public int Id { get; set; }
    public bool LlegoAlDeposito { get; set; }
    public bool YaEnviado { get; set; }
    public bool CambioRegistradoSistema { get; set; }
}
