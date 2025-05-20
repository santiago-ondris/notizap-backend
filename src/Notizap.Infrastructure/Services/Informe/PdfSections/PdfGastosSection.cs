using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

public static class PdfGastosSection
{
    public static void Compose(IContainer container, GastosResumenDto gastos)
    {
        container.Column(col =>
        {
            col.Spacing(8);
            col.Item().Text("💰 Gastos del mes").FontSize(16).Bold().Underline();

            col.Item().Text($"Total: ${gastos.TotalGastos:N2}");

            col.Item().Text("Desglose por categoría:").SemiBold();
            foreach (var item in gastos.PorCategoria!)
            {
                col.Item().Text($"• {item.Key}: ${item.Value:N2}");
            }
        });
    }
}
