using QuestPDF.Fluent;

public class InformeMensualService : IInformeMensualService
{
    private readonly WooResumenBuilder _wooBuilder;
    private readonly MercadoLibreResumenBuilder _mlBuilder;
    private readonly MailingResumenBuilder _mailingBuilder;
    private readonly GastosResumenBuilder _gastosBuilder;
    private readonly EnviosResumenBuilder _enviosBuilder;
    private readonly CambiosResumenBuilder _cambiosBuilder;
    private readonly PublicidadResumenBuilder _adsBuilder;
    private readonly InstagramResumenBuilder _instagramBuilder;

    public InformeMensualService(
        WooResumenBuilder wooBuilder,
        MercadoLibreResumenBuilder mlBuilder,
        MailingResumenBuilder mailingBuilder,
        GastosResumenBuilder gastosBuilder,
        EnviosResumenBuilder enviosBuilder,
        CambiosResumenBuilder cambiosBuilder,
        PublicidadResumenBuilder adsBuilder,
        InstagramResumenBuilder instagramBuilder
        )
    {
        _wooBuilder = wooBuilder;
        _mlBuilder = mlBuilder;
        _mailingBuilder = mailingBuilder;
        _gastosBuilder = gastosBuilder;
        _enviosBuilder = enviosBuilder;
        _cambiosBuilder = cambiosBuilder;
        _adsBuilder = adsBuilder;
        _instagramBuilder = instagramBuilder;
    }

    public async Task<byte[]> GenerarInformePdfAsync(int year, int month, bool visual = true)
    {
        // 1. Recolectar datos de todos los servicios
        var resumen = new ResumenMensualDto
        {
            Year = year,
            Month = month,
            WooCommerce = await ObtenerWooResumen(year, month),
            MercadoLibre = await ObtenerMlResumen(year, month),
            Instagram = await ObtenerInstagramResumen(year, month),
            Publicidad = await ObtenerPublicidadResumen(year, month),
            Mailing = await ObtenerMailingResumen(year, month),
            Gastos = await ObtenerGastosResumen(year, month),
            Envios = await ObtenerEnviosResumen(year, month),
            Cambios = await ObtenerCambiosResumen(year, month)
        };

        // 2. Generar el PDF (clase separada, usa QuestPDF)
        var documento = new InformeMensualPdfDocument(resumen, visual);
        return documento.GeneratePdf();
    }
    private async Task<InstagramResumenDto> ObtenerInstagramResumen(int year, int month)
    => await _instagramBuilder.ConstruirAsync(year, month);

    private Task<PublicidadResumenCompletoDto> ObtenerPublicidadResumen(int year, int month)
    => _adsBuilder.ConstruirAsync(year, month);
    private Task<GastosResumenDto> ObtenerGastosResumen(int year, int month)
    => _gastosBuilder.ConstruirAsync(year, month);

    private Task<WooResumenDto> ObtenerWooResumen(int year, int month)
    => _wooBuilder.ConstruirAsync(year, month);

    private Task<MercadoLibreResumenDto> ObtenerMlResumen(int year, int month)
    => _mlBuilder.ConstruirAsync(year, month);
    private Task<EnviosResumenDto> ObtenerEnviosResumen(int year, int month)
    => _enviosBuilder.ConstruirAsync(year, month);
    private Task<CambiosResumenDto> ObtenerCambiosResumen(int year, int month)
    => _cambiosBuilder.ConstruirAsync(year, month);
    private Task<MailingResumenGlobalDto> ObtenerMailingResumen(int year, int month)
    => _mailingBuilder.ConstruirAsync(year, month);

    public async Task<ResumenMensualDto> GenerarResumenMensualAsync(int year, int month)
    {
        var resumen = new ResumenMensualDto
        {
            Year = year,
            Month = month,
            WooCommerce = await ObtenerWooResumen(year, month),
            MercadoLibre = await ObtenerMlResumen(year, month),
            Instagram = await ObtenerInstagramResumen(year, month),
            Publicidad = await ObtenerPublicidadResumen(year, month),
            Mailing = await ObtenerMailingResumen(year, month),
            Gastos = await ObtenerGastosResumen(year, month),
            Envios = await ObtenerEnviosResumen(year, month),
            Cambios = await ObtenerCambiosResumen(year, month)
        };

        return resumen;
    }
}
