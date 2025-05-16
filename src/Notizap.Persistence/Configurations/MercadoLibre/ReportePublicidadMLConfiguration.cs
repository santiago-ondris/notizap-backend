using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ReportePublicidadMLConfiguration : IEntityTypeConfiguration<ReportePublicidadML>
{
    public void Configure(EntityTypeBuilder<ReportePublicidadML> builder)
    {
        builder.ToTable("ReportesPublicidadML");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Tipo)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(r => r.NombreCampania)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(r => r.Inversion)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasMany(r => r.Anuncios)
            .WithOne(a => a.Reporte)
            .HasForeignKey(a => a.ReportePublicidadMLId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
