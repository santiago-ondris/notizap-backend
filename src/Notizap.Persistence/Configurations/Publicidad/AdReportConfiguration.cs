using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AdReportConfiguration : IEntityTypeConfiguration<AdReport>
{
    public void Configure(EntityTypeBuilder<AdReport> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.UnidadNegocio).IsRequired();
        builder.Property(r => r.Plataforma).IsRequired();
        builder.Property(r => r.Year).IsRequired();
        builder.Property(r => r.Month).IsRequired();

        builder.HasMany(r => r.Campañas)
               .WithOne(c => c.Reporte)
               .HasForeignKey(c => c.AdReportId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
