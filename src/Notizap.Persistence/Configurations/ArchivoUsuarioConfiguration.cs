using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ArchivoUsuarioConfiguration : IEntityTypeConfiguration<ArchivoUsuario>
{
    public void Configure(EntityTypeBuilder<ArchivoUsuario> builder)
    {
        builder.ToTable("ArchivosUsuario");

        // Primary Key
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        // Usuario (Foreign Key)
        builder.Property(a => a.UsuarioId)
            .IsRequired();

        // Información del archivo
        builder.Property(a => a.NombreArchivo)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.NombreOriginal)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(a => a.RutaArchivo)
            .IsRequired()
            .HasMaxLength(500);

        // Enum como int
        builder.Property(a => a.TipoArchivo)
            .IsRequired()
            .HasConversion<int>();

        // Fechas
        builder.Property(a => a.FechaSubida)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(a => a.UltimoAcceso)
            .HasColumnType("timestamp with time zone");

        builder.Property(a => a.FechaCreacion)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(a => a.FechaModificacion)
            .HasColumnType("timestamp with time zone");

        // Estadísticas y metadatos
        builder.Property(a => a.TamañoBytes)
            .IsRequired();

        builder.Property(a => a.VecesUtilizado)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(a => a.EsFavorito)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(a => a.Descripcion)
            .HasMaxLength(500);

        builder.Property(a => a.TagsMetadata)
            .HasMaxLength(1000);

        // Campos de auditoría
        builder.Property(a => a.CreadoPor)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.ModificadoPor)
            .HasMaxLength(100);

        // Índices para optimizar consultas
        builder.HasIndex(a => a.UsuarioId)
            .HasDatabaseName("IX_ArchivosUsuario_UsuarioId");

        builder.HasIndex(a => new { a.UsuarioId, a.TipoArchivo })
            .HasDatabaseName("IX_ArchivosUsuario_Usuario_Tipo");

        builder.HasIndex(a => new { a.UsuarioId, a.Activo })
            .HasDatabaseName("IX_ArchivosUsuario_Usuario_Activo");

        builder.HasIndex(a => new { a.UsuarioId, a.EsFavorito })
            .HasDatabaseName("IX_ArchivosUsuario_Usuario_Favorito");

        builder.HasIndex(a => a.FechaSubida)
            .HasDatabaseName("IX_ArchivosUsuario_FechaSubida");

        builder.HasIndex(a => a.UltimoAcceso)
            .HasDatabaseName("IX_ArchivosUsuario_UltimoAcceso");

        builder.HasIndex(a => a.VecesUtilizado)
            .HasDatabaseName("IX_ArchivosUsuario_VecesUtilizado");

        // Relación con User
        builder.HasOne(a => a.Usuario)
            .WithMany() // Asumiendo que User no tiene navegación hacia ArchivosUsuario
            .HasForeignKey(a => a.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}