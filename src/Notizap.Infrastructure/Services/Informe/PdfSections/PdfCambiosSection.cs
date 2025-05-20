using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

public static class PdfCambiosSection
{
    public static void Compose(IContainer container, CambiosResumenDto cambios)
    {
        container.Column(col =>
        {
            col.Spacing(8);
            col.Item().Text("🔁 Cambios y Devoluciones").FontSize(16).Bold().Underline();

            col.Item().Text($"• Cambios registrados: {cambios.CantidadCambios}");
            col.Item().Text($"• Devoluciones registradas: {cambios.CantidadDevoluciones}");
        });
    }
}
