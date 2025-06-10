using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(c => c.CantidadCompras)
            .IsRequired();

        builder.Property(c => c.MontoTotalGastado)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(c => c.FechaPrimeraCompra)
            .IsRequired();

        builder.Property(c => c.FechaUltimaCompra)
            .IsRequired();

        builder.Property(c => c.Canales)
            .HasMaxLength(200);

        builder.Property(c => c.Sucursales)
            .HasMaxLength(200);

        builder.Property(c => c.Observaciones)
            .HasMaxLength(200);

        builder.HasMany(c => c.Compras)
            .WithOne(x => x.Cliente)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
