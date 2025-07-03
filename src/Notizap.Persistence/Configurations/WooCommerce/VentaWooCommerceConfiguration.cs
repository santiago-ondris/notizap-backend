using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
public class VentaWooCommerceConfiguration : IEntityTypeConfiguration<VentaWooCommerce>
{
    public void Configure(EntityTypeBuilder<VentaWooCommerce> builder)
    {
        builder.ToTable("VentasWooCommerce");
        
        // Primary Key
        builder.HasKey(v => v.Id);
        
        // Properties Configuration
        builder.Property(v => v.Id)
            .ValueGeneratedOnAdd();
            
        builder.Property(v => v.Tienda)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(v => v.Mes)
            .IsRequired();
            
        builder.Property(v => v.Año)
            .IsRequired();
            
        builder.Property(v => v.MontoFacturado)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
            
        builder.Property(v => v.UnidadesVendidas)
            .IsRequired();
            
        builder.Property(v => v.TopProductos)
            .IsRequired()
            .HasColumnType("text")
            .HasDefaultValue("[]");
            
        builder.Property(v => v.TopCategorias)
            .IsRequired()
            .HasColumnType("text")
            .HasDefaultValue("[]");
            
        builder.Property(v => v.FechaCreacion)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");
            
        builder.Property(v => v.FechaActualizacion)
            .IsRequired(false)
            .HasColumnType("timestamp with time zone");
        
        // Indexes
        builder.HasIndex(v => new { v.Tienda, v.Mes, v.Año })
            .HasDatabaseName("IX_VentasWooCommerce_Tienda_Periodo")
            .IsUnique();
            
        builder.HasIndex(v => new { v.Año, v.Mes })
            .HasDatabaseName("IX_VentasWooCommerce_Periodo");
            
        builder.HasIndex(v => v.FechaCreacion)
            .HasDatabaseName("IX_VentasWooCommerce_FechaCreacion");
        
        // Computed Properties (ignorar en la base)
        builder.Ignore(v => v.PeriodoCompleto);
        builder.Ignore(v => v.TiendaNormalizada);
    }
}