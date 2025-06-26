using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
public class VendedorVentaConfiguration : IEntityTypeConfiguration<VendedorVenta>
{
    public void Configure(EntityTypeBuilder<VendedorVenta> builder)
    {
        builder.ToTable("VendedoresVentas");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(v => v.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Email)
            .HasMaxLength(100);

        builder.Property(v => v.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(v => v.FechaCreacion)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Índices
        builder.HasIndex(v => v.Nombre)
            .HasDatabaseName("IX_VendedoresVentas_Nombre");

        builder.HasIndex(v => v.Email)
            .IsUnique()
            .HasDatabaseName("IX_VendedoresVentas_Email")
            .HasFilter("\"Email\" IS NOT NULL");

        builder.HasIndex(v => v.Activo)
            .HasDatabaseName("IX_VendedoresVentas_Activo");

        // Relaciones
        builder.HasMany(v => v.Ventas)
            .WithOne(vta => vta.Vendedor)
            .HasForeignKey(vta => vta.VendedorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}