using Microsoft.AspNetCore.Http;

public class ArchivoUsuarioDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string NombreOriginal { get; set; } = string.Empty;
    public TipoArchivo TipoArchivo { get; set; }
    public string TipoArchivoTexto { get; set; } = string.Empty; // Para display en frontend
    public DateTime FechaSubida { get; set; }
    public string TamañoFormateado { get; set; } = string.Empty; // "2.5 MB"
    public long TamañoBytes { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    public int VecesUtilizado { get; set; }
    public bool EsFavorito { get; set; }
    public string? Descripcion { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public string UsuarioNombre { get; set; } = string.Empty;
}

public class CrearArchivoUsuarioDto
{
    public string NombreArchivo { get; set; } = string.Empty;
    public TipoArchivo TipoArchivo { get; set; }
    public string? Descripcion { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public bool EsFavorito { get; set; } = false;
}

public class ActualizarArchivoUsuarioDto
{
    public string? NombreArchivo { get; set; }
    public string? Descripcion { get; set; }
    public List<string>? Tags { get; set; }
    public bool? EsFavorito { get; set; }
}

public class SubirArchivoRequest
{
    public IFormFile Archivo { get; set; } = null!;
    public TipoArchivo TipoArchivo { get; set; }
    public string? NombrePersonalizado { get; set; }
    public string? Descripcion { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public bool EsFavorito { get; set; } = false;
}

public class ListarArchivosRequest
{
    public TipoArchivo? TipoArchivo { get; set; }
    public bool? SoloFavoritos { get; set; }
    public string? BuscarTexto { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamañoPagina { get; set; } = 10;
    public string OrdenarPor { get; set; } = "FechaSubida"; // FechaSubida, Nombre, VecesUtilizado
    public bool Descendente { get; set; } = true;
}

public class ListarArchivosResponse
{
    public List<ArchivoUsuarioDto> Archivos { get; set; } = new List<ArchivoUsuarioDto>();
    public int TotalArchivos { get; set; }
    public int PaginaActual { get; set; }
    public int TotalPaginas { get; set; }
    public bool TieneSiguiente { get; set; }
    public bool TieneAnterior { get; set; }
}

public class EstadisticasArchivosDto
{
    public int TotalArchivos { get; set; }
    public long EspacioUsadoBytes { get; set; }
    public string EspacioUsadoFormateado { get; set; } = string.Empty;
    public Dictionary<string, int> ArchivosPorTipo { get; set; } = new Dictionary<string, int>();
    public ArchivoUsuarioDto? ArchivoMasUsado { get; set; }
    public ArchivoUsuarioDto? ArchivoReciente { get; set; }
}