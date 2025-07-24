using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/archivos-usuario")]
[Authorize] // Requiere autenticación
public class ArchivoUsuarioController : ControllerBase
{
    private readonly IArchivoUsuarioService _archivoUsuarioService;
    private readonly ILogger<ArchivoUsuarioController> _logger;

    public ArchivoUsuarioController(
        IArchivoUsuarioService archivoUsuarioService,
        ILogger<ArchivoUsuarioController> logger)
    {
        _archivoUsuarioService = archivoUsuarioService;
        _logger = logger;
    }

    [HttpPost("subir")]
    [SwaggerOperation(Summary = "Subir un archivo para el usuario autenticado")]
    [ProducesResponseType(typeof(ArchivoUsuarioDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(413)] // Payload Too Large
    public async Task<ActionResult<ArchivoUsuarioDto>> SubirArchivo([FromForm] SubirArchivoRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.Archivo == null || request.Archivo.Length == 0)
                return BadRequest("El archivo no puede estar vacío");

            // Validar tamaño máximo (50MB por ejemplo)
            const long tamañoMaximo = 50 * 1024 * 1024; // 50MB
            if (request.Archivo.Length > tamañoMaximo)
                return StatusCode(413, "El archivo excede el tamaño máximo permitido (50MB)");

            var usuarioId = ObtenerUsuarioId();
            var nombreUsuario = ObtenerNombreUsuario();

            var resultado = await _archivoUsuarioService.SubirArchivoAsync(request, usuarioId, nombreUsuario);

            _logger.LogInformation("📁 Usuario {UsuarioId} subió archivo: {ArchivoId}", 
                usuarioId, resultado.Id);

            return Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error subiendo archivo para usuario {UsuarioId}", ObtenerUsuarioId());
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Listar archivos del usuario con filtros y paginación")]
    [ProducesResponseType(typeof(ListarArchivosResponse), 200)]
    public async Task<ActionResult<ListarArchivosResponse>> ListarArchivos([FromQuery] ListarArchivosRequest request)
    {
        try
        {
            var usuarioId = ObtenerUsuarioId();
            var resultado = await _archivoUsuarioService.ListarArchivosAsync(request, usuarioId);

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error listando archivos para usuario {UsuarioId}", ObtenerUsuarioId());
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obtener un archivo específico por ID")]
    [ProducesResponseType(typeof(ArchivoUsuarioDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ArchivoUsuarioDto>> ObtenerPorId(int id)
    {
        try
        {
            var usuarioId = ObtenerUsuarioId();
            var archivo = await _archivoUsuarioService.ObtenerPorIdAsync(id, usuarioId);

            if (archivo == null)
                return NotFound("Archivo no encontrado");

            return Ok(archivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obteniendo archivo {ArchivoId} para usuario {UsuarioId}", 
                id, ObtenerUsuarioId());
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Actualizar información de un archivo")]
    [ProducesResponseType(typeof(ArchivoUsuarioDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ArchivoUsuarioDto>> Actualizar(int id, [FromBody] ActualizarArchivoUsuarioDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuarioId = ObtenerUsuarioId();
            var nombreUsuario = ObtenerNombreUsuario();

            var resultado = await _archivoUsuarioService.ActualizarAsync(id, dto, usuarioId, nombreUsuario);

            if (resultado == null)
                return NotFound("Archivo no encontrado");

            _logger.LogInformation("✏️ Usuario {UsuarioId} actualizó archivo: {ArchivoId}", usuarioId, id);

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error actualizando archivo {ArchivoId} para usuario {UsuarioId}", 
                id, ObtenerUsuarioId());
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Eliminar un archivo (soft delete)")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var usuarioId = ObtenerUsuarioId();
            var eliminado = await _archivoUsuarioService.EliminarAsync(id, usuarioId);

            if (!eliminado)
                return NotFound("Archivo no encontrado");

            _logger.LogInformation("🗑️ Usuario {UsuarioId} eliminó archivo: {ArchivoId}", usuarioId, id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error eliminando archivo {ArchivoId} para usuario {UsuarioId}", 
                id, ObtenerUsuarioId());
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpPatch("{id}/favorito")]
    [SwaggerOperation(Summary = "Marcar/desmarcar archivo como favorito")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarcarFavorito(int id, [FromBody] bool esFavorito)
    {
        try
        {
            var usuarioId = ObtenerUsuarioId();
            var resultado = await _archivoUsuarioService.MarcarComoFavoritoAsync(id, usuarioId, esFavorito);

            if (!resultado)
                return NotFound("Archivo no encontrado");

            _logger.LogInformation("⭐ Usuario {UsuarioId} marcó archivo {ArchivoId} como favorito: {EsFavorito}", 
                usuarioId, id, esFavorito);

            return Ok(new { success = true, esFavorito });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error marcando favorito archivo {ArchivoId} para usuario {UsuarioId}", 
                id, ObtenerUsuarioId());
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpGet("{id}/descargar")]
    [SwaggerOperation(Summary = "Descargar archivo (registra uso automáticamente)")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DescargarArchivo(int id)
    {
        try
        {
            var usuarioId = ObtenerUsuarioId();
            
            // Obtener información del archivo primero
            var archivoInfo = await _archivoUsuarioService.ObtenerPorIdAsync(id, usuarioId);
            if (archivoInfo == null)
                return NotFound("Archivo no encontrado");

            // Descargar contenido
            var contenido = await _archivoUsuarioService.DescargarArchivoAsync(id, usuarioId);

            _logger.LogInformation("⬇️ Usuario {UsuarioId} descargó archivo: {ArchivoId}", usuarioId, id);

            // Determinar content type basado en extensión
            var contentType = ObtenerContentType(archivoInfo.NombreOriginal);

            return File(contenido, contentType, archivoInfo.NombreOriginal);
        }
        catch (FileNotFoundException)
        {
            return NotFound("Archivo no encontrado en el sistema de archivos");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error descargando archivo {ArchivoId} para usuario {UsuarioId}", 
                id, ObtenerUsuarioId());
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpPatch("{id}/renombrar")]
    [SwaggerOperation(Summary = "Renombrar un archivo")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RenombrarArchivo(int id, [FromBody] string nuevoNombre)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nuevoNombre))
                return BadRequest("El nuevo nombre no puede estar vacío");

            var usuarioId = ObtenerUsuarioId();
            var resultado = await _archivoUsuarioService.RenombrarArchivoAsync(id, usuarioId, nuevoNombre.Trim());

            if (!resultado)
                return NotFound("Archivo no encontrado");

            _logger.LogInformation("✏️ Usuario {UsuarioId} renombró archivo {ArchivoId} a: {NuevoNombre}", 
                usuarioId, id, nuevoNombre);

            return Ok(new { success = true, nuevoNombre });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error renombrando archivo {ArchivoId} para usuario {UsuarioId}", 
                id, ObtenerUsuarioId());
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpGet("estadisticas")]
    [SwaggerOperation(Summary = "Obtener estadísticas de archivos del usuario")]
    [ProducesResponseType(typeof(EstadisticasArchivosDto), 200)]
    public async Task<ActionResult<EstadisticasArchivosDto>> ObtenerEstadisticas()
    {
        try
        {
            var usuarioId = ObtenerUsuarioId();
            var estadisticas = await _archivoUsuarioService.ObtenerEstadisticasAsync(usuarioId);

            return Ok(estadisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obteniendo estadísticas para usuario {UsuarioId}", ObtenerUsuarioId());
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpGet("favoritos")]
    [SwaggerOperation(Summary = "Obtener archivos favoritos del usuario")]
    [ProducesResponseType(typeof(List<ArchivoUsuarioDto>), 200)]
    public async Task<ActionResult<List<ArchivoUsuarioDto>>> ObtenerFavoritos()
    {
        try
        {
            var usuarioId = ObtenerUsuarioId();
            var favoritos = await _archivoUsuarioService.ObtenerFavoritosAsync(usuarioId);

            return Ok(favoritos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obteniendo favoritos para usuario {UsuarioId}", ObtenerUsuarioId());
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpGet("recientes")]
    [SwaggerOperation(Summary = "Obtener archivos recientes del usuario")]
    [ProducesResponseType(typeof(List<ArchivoUsuarioDto>), 200)]
    public async Task<ActionResult<List<ArchivoUsuarioDto>>> ObtenerRecientes([FromQuery] int cantidad = 5)
    {
        try
        {
            var usuarioId = ObtenerUsuarioId();
            var recientes = await _archivoUsuarioService.ObtenerRecientesAsync(usuarioId, cantidad);

            return Ok(recientes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obteniendo recientes para usuario {UsuarioId}", ObtenerUsuarioId());
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpGet("por-tipo/{tipoArchivo}")]
    [SwaggerOperation(Summary = "Obtener archivos por tipo específico")]
    [ProducesResponseType(typeof(List<ArchivoUsuarioDto>), 200)]
    public async Task<ActionResult<List<ArchivoUsuarioDto>>> ObtenerPorTipo(TipoArchivo tipoArchivo)
    {
        try
        {
            var usuarioId = ObtenerUsuarioId();
            var archivos = await _archivoUsuarioService.ObtenerPorTipoAsync(usuarioId, tipoArchivo);

            return Ok(archivos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obteniendo archivos por tipo {TipoArchivo} para usuario {UsuarioId}", 
                tipoArchivo, ObtenerUsuarioId());
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpPost("{id}/registrar-uso")]
    [SwaggerOperation(Summary = "Registrar uso de un archivo (para estadísticas)")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RegistrarUso(int id)
    {
        try
        {
            var usuarioId = ObtenerUsuarioId();
            
            // Validar que el archivo existe y pertenece al usuario
            if (!await _archivoUsuarioService.ValidarPropietarioAsync(id, usuarioId))
                return NotFound("Archivo no encontrado");

            await _archivoUsuarioService.RegistrarUsoAsync(id, usuarioId);

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error registrando uso archivo {ArchivoId} para usuario {UsuarioId}", 
                id, ObtenerUsuarioId());
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // Métodos privados de utilidad
    private int ObtenerUsuarioId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new UnauthorizedAccessException("Usuario no autenticado o ID inválido");
        }
        return userId;
    }

    private string ObtenerNombreUsuario()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value ?? 
               User.FindFirst("username")?.Value ?? 
               "Usuario";
    }

    private static string ObtenerContentType(string nombreArchivo)
    {
        var extension = Path.GetExtension(nombreArchivo).ToLower();
        return extension switch
        {
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream"
        };
    }
}