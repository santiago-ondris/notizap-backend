// DTOs/ResumenMensualDto.cs
public class ResumenMensualDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public WooResumenDto? WooCommerce { get; set; }
    public MercadoLibreResumenDto? MercadoLibre { get; set; }
    public InstagramResumenDto? Instagram { get; set; }
    public PublicidadResumenCompletoDto? Publicidad { get; set; }
    public MailingResumenGlobalDto? Mailing { get; set; }
    public GastosResumenDto? Gastos { get; set; }
    public EnviosResumenDto? Envios { get; set; }
    public CambiosResumenDto? Cambios { get; set; }
}
