using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public static class PdfMailingSection
{
    public static void Compose(IContainer container, MailingResumenGlobalDto mailing)
    {
        container.Column(col =>
        {
            col.Spacing(8);
            col.Item().Text("📧 Campañas de Mailing (Mailchimp)").FontSize(16).Bold().Underline();

            ComposeCuenta(col, "Montella", mailing.Montella);
            ComposeCuenta(col, "Alenka", mailing.Alenka);
        });
    }

    private static void ComposeCuenta(ColumnDescriptor col, string cuenta, MailingResumenDto resumen)
    {
        col.Item().Text($"✉️ Cuenta: {cuenta}").FontSize(13).Bold();

        col.Item().Text($"• Mails enviados: {resumen.TotalMailsEnviados}");

        col.Item().Text($"• Mejor open rate: {resumen.MejorOpenRateCampania} ({resumen.OpenRate:P1})");
        col.Item().Text($"• Mejor click rate: {resumen.MejorClickRateCampania} ({resumen.ClickRate:P1})");
        col.Item().Text($"• Mejor campaña por conversiones: {resumen.MejorConversionCampania} ({resumen.Conversiones} conversiones)");

        col.Item().PaddingBottom(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
    }
}
