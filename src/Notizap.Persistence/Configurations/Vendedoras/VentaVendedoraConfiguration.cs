using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
public class VentaVendedoraConfiguration : IEntityTypeConfiguration<VentaVendedora>
{
    public void Configure(EntityTypeBuilder<VentaVendedora> builder)
    {
        builder.ToTable("VentasVendedoras");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(v => v.SucursalId)
            .IsRequired();

        builder.Property(v => v.VendedorId)
            .IsRequired();

        builder.Property(v => v.Producto)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.Fecha)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(v => v.Cantidad)
            .IsRequired();

        builder.Property(v => v.Total)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasPrecision(18, 2);

        builder.Property(v => v.Turno)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(v => v.EsProductoDescuento)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(v => v.FechaCreacion)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Índices para optimizar consultas
        builder.HasIndex(v => v.Fecha)
            .HasDatabaseName("IX_VentasVendedoras_Fecha");

        builder.HasIndex(v => new { v.SucursalId, v.Fecha })
            .HasDatabaseName("IX_VentasVendedoras_Sucursal_Fecha");

        builder.HasIndex(v => new { v.VendedorId, v.Fecha })
            .HasDatabaseName("IX_VentasVendedoras_Vendedor_Fecha");

        builder.HasIndex(v => new { v.Fecha, v.Turno })
            .HasDatabaseName("IX_VentasVendedoras_Fecha_Turno");

        builder.HasIndex(v => v.EsProductoDescuento)
            .HasDatabaseName("IX_VentasVendedoras_EsProductoDescuento");

        builder.HasIndex(v => new { v.SucursalId, v.VendedorId, v.Fecha })
            .HasDatabaseName("IX_VentasVendedoras_Sucursal_Vendedor_Fecha");

        // Relaciones
        builder.HasOne(v => v.Sucursal)
            .WithMany(s => s.Ventas)
            .HasForeignKey(v => v.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Vendedor)
            .WithMany(vdr => vdr.Ventas)
            .HasForeignKey(v => v.VendedorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}