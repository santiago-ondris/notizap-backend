using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public static class PdfInstagramSection
{
    public static void Compose(IContainer container, InstagramResumenDto ig)
    {
        container.Column(col =>
        {
            col.Spacing(8);
            col.Item().Text("📷 Instagram").FontSize(16).Bold().Underline();

            ComposeCuenta(col, "Montella", ig.CuentaMontella);
            ComposeCuenta(col, "Alenka", ig.CuentaAlenka);
            ComposeCuenta(col, "Kids", ig.CuentaKids);
        });
    }

    private static void ComposeCuenta(ColumnDescriptor col, string nombre, InstagramCuentaResumenDto resumen)
    {
        col.Item().Text($"📱 Cuenta: {nombre}").Bold().FontSize(13);

        col.Item().Row(row =>
        {
            row.AutoItem().Text($"Reels: {resumen.CantidadReels}");
            row.Spacing(20);
            row.AutoItem().Text($"Posteos: {resumen.CantidadPosteos}");
            row.Spacing(20);
            row.AutoItem().Text($"Historias: {resumen.CantidadHistorias}");
            row.Spacing(20);
            row.AutoItem().Text($"Nuevos seguidores: {resumen.NuevosSeguidores}");
        });

        if (resumen.MasVisto != null)
        {
            col.Item().Text($"🎥 Reel más visto - {resumen.MasVisto.FechaPublicacion:dd/MM} | Vistas: {resumen.MasVisto.Views} | Likes: {resumen.MasVisto.Likes}");
        }

        if (resumen.MasLikeado != null)
        {
            col.Item().Text($"❤️ Reel con más likes - {resumen.MasLikeado.FechaPublicacion:dd/MM} | Likes: {resumen.MasLikeado.Likes} | Vistas: {resumen.MasLikeado.Views}");
        }

        col.Item().PaddingBottom(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
    }
}
