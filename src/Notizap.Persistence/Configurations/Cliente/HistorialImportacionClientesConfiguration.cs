using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class HistorialImportacionClientesConfiguration : IEntityTypeConfiguration<HistorialImportacionClientes>
{
    public void Configure(EntityTypeBuilder<HistorialImportacionClientes> builder)
    {
        builder.ToTable("HistorialImportacionClientes");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.NombreArchivo)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(h => h.FechaImportacion)
            .IsRequired();

        builder.Property(h => h.CantidadClientesNuevos)
            .IsRequired();

        builder.Property(h => h.CantidadComprasNuevas)
            .IsRequired();
    }
}
