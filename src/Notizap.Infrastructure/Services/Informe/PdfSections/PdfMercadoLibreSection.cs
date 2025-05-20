using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

public static class PdfMercadoLibreSection
{
    public static void Compose(IContainer container, MercadoLibreResumenDto ml)
    {
        container.Column(col =>
        {
            col.Spacing(8);
            col.Item().Text("📦 MercadoLibre").FontSize(16).Bold().Underline();

            col.Item().Row(row =>
            {
                row.AutoItem().Text($"• Facturación: ${ml.Total:N2}");
                row.Spacing(25);
                row.AutoItem().Text($"Unidades vendidas: {ml.Unidades}");
            });

            col.Item().Text("Top 3 productos vendidos:").SemiBold();
            foreach (var producto in ml.TopProductos!)
                col.Item().Text($"- {producto}");

            col.Item().PaddingTop(10).Text("📢 Inversión Publicitaria").Bold();

            col.Item().PaddingTop(10).Text("📢 Inversión Publicitaria").Bold();

            col.Item().Text($"• Total: ${ml.InversionTotal:N2}");
            col.Item().Text($"• Product Ads: ${ml.InversionProductAds:N2} ({ml.CampañasProductAds} campañas)");
            col.Item().Text($"• Brand Ads: ${ml.InversionBrandAds:N2} ({ml.CampañasBrandAds} campañas)");
            col.Item().Text($"• Display Ads: ${ml.InversionDisplayAds:N2} ({ml.CampañasDisplayAds} campañas)");
        });
    }
}
