using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ComisionOnlineConfiguration : IEntityTypeConfiguration<ComisionOnline>
{
    public void Configure(EntityTypeBuilder<ComisionOnline> builder)
    {
        // Tabla
        builder.ToTable("ComisionesOnline");

        // Clave primaria
        builder.HasKey(c => c.Id);

        // Propiedades
        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Mes)
            .IsRequired()
            .HasComment("Mes del período (1-12)");

        builder.Property(c => c.Año)
            .IsRequired()
            .HasComment("Año del período");

        builder.Property(c => c.TotalSinNC)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasComment("Total facturado sin notas de crédito");

        builder.Property(c => c.MontoAndreani)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasComment("Monto pagado a Andreani");

        builder.Property(c => c.MontoOCA)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasComment("Monto pagado a OCA");

        builder.Property(c => c.MontoCaddy)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasComment("Monto pagado a Caddy");

        builder.Property(c => c.FechaCreacion)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasComment("Fecha de creación del registro");

        builder.Property(c => c.FechaActualizacion)
            .HasComment("Fecha de última actualización");

        // Índices
        builder.HasIndex(c => new { c.Mes, c.Año })
            .IsUnique()
            .HasDatabaseName("IX_ComisionesOnline_Periodo");

        builder.HasIndex(c => c.Año)
            .HasDatabaseName("IX_ComisionesOnline_Año");

        builder.HasIndex(c => c.FechaCreacion)
            .HasDatabaseName("IX_ComisionesOnline_FechaCreacion");
    }
}