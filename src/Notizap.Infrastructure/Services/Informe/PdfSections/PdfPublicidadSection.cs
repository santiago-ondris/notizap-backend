using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

public static class PdfPublicidadSection
{
    public static void Compose(IContainer container, PublicidadResumenCompletoDto publicidad)
    {
        container.Column(col =>
        {
            col.Spacing(8);
            col.Item().Text("📢 Publicidad Online").FontSize(16).Bold().Underline();

            col.Item().Text($"💰 Inversión total: ${publicidad.InversionTotal:N2}").Bold();

            col.Item().Row(row =>
            {
                row.AutoItem().Text($"Meta - Montella: ${publicidad.InversionMetaMontella:N2}");
                row.Spacing(15);
                row.AutoItem().Text($"Meta - Alenka: ${publicidad.InversionMetaAlenka:N2}");
                row.Spacing(15);
                row.AutoItem().Text($"Meta - Kids: ${publicidad.InversionMetaKids:N2}");
                row.Spacing(15);
                row.AutoItem().Text($"Google - Montella: ${publicidad.InversionGoogleMontella:N2}");
            });

            if (publicidad.CampañasMeta.Any())
            {
                col.Item().PaddingTop(10).Text("📊 Campañas Meta").SemiBold();
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3); // nombre
                        c.RelativeColumn(2); // unidad
                        c.RelativeColumn(1); // $
                        c.RelativeColumn(1); // followers
                        c.RelativeColumn(1); // resultados
                    });

                    table.Header(h =>
                    {
                        h.Cell().Text("Nombre").Bold();
                        h.Cell().Text("Unidad").Bold();
                        h.Cell().Text("Inversión").Bold();
                        h.Cell().Text("Followers").Bold();
                        h.Cell().Text("Resultados").Bold();
                    });

                    foreach (var c in publicidad.CampañasMeta)
                    {
                        table.Cell().Text(c.Nombre);
                        table.Cell().Text(c.UnidadNegocio);
                        table.Cell().Text($"${c.Inversion:N2}");
                        table.Cell().Text(c.Seguidores?.ToString() ?? "-");
                        table.Cell().Text(c.Resultados?.ToString() ?? "-");
                    }
                });
            }

            if (publicidad.CampañasGoogle.Any())
            {
                col.Item().PaddingTop(10).Text("📊 Campañas Google").SemiBold();
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.RelativeColumn(2);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                    });

                    table.Header(h =>
                    {
                        h.Cell().Text("Nombre").Bold();
                        h.Cell().Text("Unidad").Bold();
                        h.Cell().Text("Inversión").Bold();
                        h.Cell().Text("Followers").Bold();
                        h.Cell().Text("Resultados").Bold();
                    });

                    foreach (var c in publicidad.CampañasGoogle)
                    {
                        table.Cell().Text(c.Nombre);
                        table.Cell().Text(c.UnidadNegocio);
                        table.Cell().Text($"${c.Inversion:N2}");
                        table.Cell().Text(c.Seguidores?.ToString() ?? "-");
                        table.Cell().Text(c.Resultados?.ToString() ?? "-");
                    }
                });
            }
        });
    }
}
