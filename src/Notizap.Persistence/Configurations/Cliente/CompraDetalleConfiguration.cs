using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CompraDetalleConfiguration : IEntityTypeConfiguration<CompraDetalle>
{
    public void Configure(EntityTypeBuilder<CompraDetalle> builder)
    {
        builder.ToTable("CompraDetalles");

        builder.HasKey(cd => cd.Id);

        builder.Property(cd => cd.Producto)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(cd => cd.Marca)
            .HasMaxLength(70);

        builder.Property(cd => cd.Categoria)
            .HasMaxLength(70);

        builder.Property(cd => cd.Cantidad)
            .IsRequired();

        builder.Property(cd => cd.Total)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
    }
}
