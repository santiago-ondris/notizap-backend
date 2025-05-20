using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

public static class PdfEnviosSection
{
    public static void Compose(IContainer container, EnviosResumenDto envios)
    {
        container.Column(col =>
        {
            col.Spacing(8);
            col.Item().Text("📦 Envíos del mes").FontSize(16).Bold().Underline();

            col.Item().Text($"Total de envíos: {envios.TotalEnvios}");

            col.Item().Text("Cantidad por tipo de envío:").SemiBold();
            foreach (var tipo in envios.PorTipo!)
            {
                col.Item().Text($"• {tipo.Key}: {tipo.Value}");
            }
        });
    }
}
