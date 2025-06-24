using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
public class DevolucionMercadoLibreConfiguration : IEntityTypeConfiguration<DevolucionMercadoLibre>
{
    public void Configure(EntityTypeBuilder<DevolucionMercadoLibre> builder)
    {
        builder.ToTable("DevolucionesMercadoLibre");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(d => d.Fecha)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(d => d.Cliente)
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode(true);

        builder.Property(d => d.Pedido)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode(true);    

        builder.Property(d => d.Modelo)
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode(true);

        builder.Property(d => d.NotaCreditoEmitida)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(d => d.FechaCreacion)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(d => d.FechaActualizacion)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(d => d.Fecha)
            .HasDatabaseName("IX_DevolucionesMercadoLibre_Fecha");

        builder.HasIndex(d => d.Cliente)
            .HasDatabaseName("IX_DevolucionesMercadoLibre_Cliente");

        builder.HasIndex(d => d.Pedido)
            .HasDatabaseName("IX_DevolucionesMercadoLibre_Pedido");    

        builder.HasIndex(d => d.NotaCreditoEmitida)
            .HasDatabaseName("IX_DevolucionesMercadoLibre_NotaCredito");

        builder.HasIndex(d => new { d.Fecha, d.NotaCreditoEmitida })
            .HasDatabaseName("IX_DevolucionesMercadoLibre_FechaNotaCredito");

        builder.HasIndex(d => d.Modelo)
            .HasDatabaseName("IX_DevolucionesMercadoLibre_Modelo");
    }
}