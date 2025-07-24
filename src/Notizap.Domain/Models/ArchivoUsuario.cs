public class ArchivoUsuario
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string NombreOriginal { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public TipoArchivo TipoArchivo { get; set; }
    public DateTime FechaSubida { get; set; }
    public long TamañoBytes { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    public int VecesUtilizado { get; set; }
    public bool EsFavorito { get; set; }
    public bool Activo { get; set; }
    public string? Descripcion { get; set; }
    public string? TagsMetadata { get; set; } // JSON para guardar tags/metadata adicional
    
    // Propiedades de auditoría
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public string? ModificadoPor { get; set; }

    // Navegación
    public User Usuario { get; set; } = null!;
}