using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CompraConfiguration : IEntityTypeConfiguration<Compra>
{
    public void Configure(EntityTypeBuilder<Compra> builder)
    {
        builder.ToTable("Compras");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Fecha)
            .IsRequired();

        builder.Property(c => c.Canal)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(c => c.Sucursal)
            .HasMaxLength(50);

        builder.Property(c => c.Total)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasMany(c => c.Detalles)
            .WithOne(x => x.Compra)
            .HasForeignKey(x => x.CompraId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
