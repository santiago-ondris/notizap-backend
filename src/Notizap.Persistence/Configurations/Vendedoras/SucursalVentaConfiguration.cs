using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
public class SucursalVentaConfiguration : IEntityTypeConfiguration<SucursalVenta>
{
    public void Configure(EntityTypeBuilder<SucursalVenta> builder)
    {
        builder.ToTable("SucursalesVentas");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(s => s.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.AbreSabadoTarde)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.FechaCreacion)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Índices
        builder.HasIndex(s => s.Nombre)
            .IsUnique()
            .HasDatabaseName("IX_SucursalesVentas_Nombre");

        // Relaciones
        builder.HasMany(s => s.Ventas)
            .WithOne(v => v.Sucursal)
            .HasForeignKey(v => v.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed data para sucursales con horario especial
        builder.HasData(
            new SucursalVenta 
            { 
                Id = 1, 
                Nombre = "25 de mayo", 
                AbreSabadoTarde = false,
                FechaCreacion = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new SucursalVenta 
            { 
                Id = 2, 
                Nombre = "DEAN FUNES", 
                AbreSabadoTarde = false,
                FechaCreacion = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}