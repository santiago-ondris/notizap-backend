public class OcaSettings
{
    public string Usuario { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string NumeroCuenta { get; set; } = default!;
    public int IdCentroImposicionDestino { get; internal set; }
}