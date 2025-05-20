using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class InformeMensualPdfDocument : IDocument
{
    private readonly ResumenMensualDto _resumen;
    private readonly bool _modoVisual;

    public InformeMensualPdfDocument(ResumenMensualDto resumen, bool modoVisual = true)
    {
        _resumen = resumen;
        _modoVisual = modoVisual;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header().Element(ComposeEncabezado);
                page.Content().Element(ComposeContenido);
                page.Footer().AlignCenter().Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}");
            });
    }

    private void ComposeEncabezado(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Informe Mensual Global").FontSize(20).Bold();
                col.Item().Text($"Período: {_resumen.Month:00}/{_resumen.Year}");
            });

            if (_modoVisual)
            {
                row.ConstantItem(60).Image(Placeholders.Image(60, 60));
            }
        });
    }

    private void ComposeContenido(IContainer container)
    {
        container
            .PaddingTop(20)
            .Column(column =>
            {
                column.Spacing(15);

                column.Item().Element(c => PdfWooSection.Compose(c, _resumen.WooCommerce!));
                column.Item().Element(c => PdfMercadoLibreSection.Compose(c, _resumen.MercadoLibre!));
                column.Item().Element(c => PdfInstagramSection.Compose(c, _resumen.Instagram!));
                column.Item().Element(c => PdfPublicidadSection.Compose(c, _resumen.Publicidad!));
                column.Item().Element(c => PdfMailingSection.Compose(c, _resumen.Mailing!));
                column.Item().Element(c => PdfGastosSection.Compose(c, _resumen.Gastos!));
                column.Item().Element(c => PdfEnviosSection.Compose(c, _resumen.Envios!));
                column.Item().Element(c => PdfCambiosSection.Compose(c, _resumen.Cambios!));
            });
    }

}
