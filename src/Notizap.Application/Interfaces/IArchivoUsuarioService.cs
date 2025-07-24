public interface IArchivoUsuarioService
{
    // Operaciones CRUD básicas
    Task<ArchivoUsuarioDto> SubirArchivoAsync(SubirArchivoRequest request, int usuarioId, string creadoPor);
    Task<ListarArchivosResponse> ListarArchivosAsync(ListarArchivosRequest request, int usuarioId);
    Task<ArchivoUsuarioDto?> ObtenerPorIdAsync(int archivoId, int usuarioId);
    Task<ArchivoUsuarioDto?> ActualizarAsync(int archivoId, ActualizarArchivoUsuarioDto dto, int usuarioId, string modificadoPor);
    Task<bool> EliminarAsync(int archivoId, int usuarioId);

    // Operaciones de gestión
    Task<bool> MarcarComoFavoritoAsync(int archivoId, int usuarioId, bool esFavorito);
    Task<byte[]> DescargarArchivoAsync(int archivoId, int usuarioId);
    Task<bool> RenombrarArchivoAsync(int archivoId, int usuarioId, string nuevoNombre);

    // Estadísticas y uso
    Task RegistrarUsoAsync(int archivoId, int usuarioId);
    Task<EstadisticasArchivosDto> ObtenerEstadisticasAsync(int usuarioId);
    Task<List<ArchivoUsuarioDto>> ObtenerFavoritosAsync(int usuarioId);
    Task<List<ArchivoUsuarioDto>> ObtenerRecientesAsync(int usuarioId, int cantidad = 5);
    Task<List<ArchivoUsuarioDto>> ObtenerPorTipoAsync(int usuarioId, TipoArchivo tipoArchivo);

    // Validación y metadata
    Task<bool> ValidarPropietarioAsync(int archivoId, int usuarioId);
    Task<bool> ExisteArchivoAsync(int archivoId);
    Task<long> ObtenerEspacioUsadoPorUsuarioAsync(int usuarioId);
}