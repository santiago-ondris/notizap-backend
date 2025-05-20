using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

public static class PdfWooSection
{
    public static void Compose(IContainer container, WooResumenDto woo)
    {
        container.Column(col =>
        {
            col.Spacing(8);
            col.Item().Text("🛒 WooCommerce").FontSize(16).Bold().Underline();

            ComposeTienda(col, "Montella", woo.TotalMontella, woo.UnidadesMontella, woo.TopProductosMontella);
            ComposeTienda(col, "Alenka", woo.TotalAlenka, woo.UnidadesAlenka, woo.TopProductosAlenka);
        });
    }

    private static void ComposeTienda(ColumnDescriptor col, string nombre, decimal total, int unidades, List<string> topProductos)
    {
        col.Item().Text($"{nombre}").Bold().FontSize(13);

        col.Item().Row(row =>
        {
            row.AutoItem().Text($"• Facturación: ${total:N2}");
            row.Spacing(25);
            row.AutoItem().Text($"Unidades vendidas: {unidades}");
        });

        col.Item().Text("Top 3 productos:").SemiBold();
        foreach (var producto in topProductos)
        {
            col.Item().Text($"- {producto}");
        }
    }
}
