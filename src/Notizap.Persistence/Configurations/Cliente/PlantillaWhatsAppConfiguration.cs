using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PlantillaWhatsAppConfiguration : IEntityTypeConfiguration<PlantillaWhatsApp>
{
    public void Configure(EntityTypeBuilder<PlantillaWhatsApp> builder)
    {
        builder.ToTable("PlantillasWhatsApp");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Mensaje)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(p => p.Descripcion)
            .HasMaxLength(200);

        builder.Property(p => p.Categoria)
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue("General");

        builder.Property(p => p.FechaCreacion)
            .IsRequired();

        builder.Property(p => p.CreadoPor)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Activa)
            .IsRequired()
            .HasDefaultValue(true);

        // Índices para optimizar consultas
        builder.HasIndex(p => p.Categoria);
        builder.HasIndex(p => p.Activa);
        builder.HasIndex(p => new { p.Activa, p.Categoria });
    }
}