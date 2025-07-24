using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class ArchivoUsuarioService : IArchivoUsuarioService
{
    private readonly NotizapDbContext _context;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ArchivoUsuarioService> _logger;
    private readonly string _rutaArchivos;

    public ArchivoUsuarioService(
        NotizapDbContext context,
        IMapper mapper,
        IConfiguration configuration,
        ILogger<ArchivoUsuarioService> logger)
    {
        _context = context;
        _mapper = mapper;
        _configuration = configuration;
        _logger = logger;
        
        // Configurar ruta de archivos desde appsettings
        _rutaArchivos = _configuration["ArchivosUsuario:RutaBase"] ?? 
                        Path.Combine(Directory.GetCurrentDirectory(), "uploads", "archivos-usuario");
        
        // Crear directorio si no existe
        if (!Directory.Exists(_rutaArchivos))
        {
            Directory.CreateDirectory(_rutaArchivos);
        }
    }

    public async Task<ArchivoUsuarioDto> SubirArchivoAsync(SubirArchivoRequest request, int usuarioId, string creadoPor)
    {
        try
        {
            _logger.LogInformation("📁 Subiendo archivo para usuario {UsuarioId}: {NombreArchivo}", 
                usuarioId, request.Archivo.FileName);

            // Validaciones
            if (request.Archivo == null || request.Archivo.Length == 0)
                throw new ArgumentException("El archivo no puede estar vacío");

            if (!EsExtensionPermitida(request.Archivo.FileName))
                throw new ArgumentException("Tipo de archivo no permitido");

            // Generar nombre único para el archivo
            var extension = Path.GetExtension(request.Archivo.FileName);
            var nombreUnico = $"{Guid.NewGuid()}{extension}";
            var rutaCompleta = Path.Combine(_rutaArchivos, nombreUnico);

            // Crear directorio del usuario si no existe
            var directorioUsuario = Path.Combine(_rutaArchivos, usuarioId.ToString());
            if (!Directory.Exists(directorioUsuario))
            {
                Directory.CreateDirectory(directorioUsuario);
            }

            rutaCompleta = Path.Combine(directorioUsuario, nombreUnico);

            // Guardar archivo físicamente
            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await request.Archivo.CopyToAsync(stream);
            }

            // Crear entidad
            var archivo = new ArchivoUsuario
            {
                UsuarioId = usuarioId,
                NombreArchivo = request.NombrePersonalizado ?? 
                                Path.GetFileNameWithoutExtension(request.Archivo.FileName),
                NombreOriginal = request.Archivo.FileName,
                RutaArchivo = rutaCompleta,
                TipoArchivo = request.TipoArchivo,
                FechaSubida = DateTime.UtcNow,
                TamañoBytes = request.Archivo.Length,
                VecesUtilizado = 0,
                EsFavorito = request.EsFavorito,
                Activo = true,
                Descripcion = request.Descripcion,
                TagsMetadata = request.Tags.Any() ? JsonConvert.SerializeObject(request.Tags) : null,
                FechaCreacion = DateTime.UtcNow,
                CreadoPor = creadoPor
            };

            _context.ArchivosUsuario.Add(archivo);
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Archivo subido exitosamente: {ArchivoId}", archivo.Id);

            return await MapearArchivoUsuarioDto(archivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error subiendo archivo para usuario {UsuarioId}", usuarioId);
            throw;
        }
    }

    public async Task<ListarArchivosResponse> ListarArchivosAsync(ListarArchivosRequest request, int usuarioId)
    {
        // Importante: declarar query explícitamente como IQueryable<ArchivoUsuario>
        IQueryable<ArchivoUsuario> query = _context.ArchivosUsuario
            .Where(a => a.UsuarioId == usuarioId && a.Activo)
            .Include(a => a.Usuario);

        // Filtros
        if (request.TipoArchivo.HasValue)
            query = query.Where(a => a.TipoArchivo == request.TipoArchivo.Value);

        if (request.SoloFavoritos == true)
            query = query.Where(a => a.EsFavorito);

        if (!string.IsNullOrWhiteSpace(request.BuscarTexto))
        {
            var busqueda = request.BuscarTexto.ToLower();
            query = query.Where(a =>
                a.NombreArchivo.ToLower().Contains(busqueda) ||
                a.NombreOriginal.ToLower().Contains(busqueda) ||
                (a.Descripcion != null && a.Descripcion.ToLower().Contains(busqueda)));
        }

        // Ordenamiento (OrderBy devuelve IOrderedQueryable<ArchivoUsuario>, que sigue siendo IQueryable<ArchivoUsuario>)
        query = request.OrdenarPor.ToLower() switch
        {
            "nombre" => request.Descendente ?
                query.OrderByDescending(a => a.NombreArchivo) :
                query.OrderBy(a => a.NombreArchivo),
            "vecesutilizado" => request.Descendente ?
                query.OrderByDescending(a => a.VecesUtilizado) :
                query.OrderBy(a => a.VecesUtilizado),
            "tamaño" => request.Descendente ?
                query.OrderByDescending(a => a.TamañoBytes) :
                query.OrderBy(a => a.TamañoBytes),
            _ => request.Descendente ?
                query.OrderByDescending(a => a.FechaSubida) :
                query.OrderBy(a => a.FechaSubida)
        };

        // Paginación
        var totalArchivos = await query.CountAsync();
        var archivos = await query
            .Skip((request.Pagina - 1) * request.TamañoPagina)
            .Take(request.TamañoPagina)
            .ToListAsync();

        var archivosDto = new List<ArchivoUsuarioDto>();
        foreach (var archivo in archivos)
        {
            archivosDto.Add(await MapearArchivoUsuarioDto(archivo));
        }

        return new ListarArchivosResponse
        {
            Archivos = archivosDto,
            TotalArchivos = totalArchivos,
            PaginaActual = request.Pagina,
            TotalPaginas = (int)Math.Ceiling((double)totalArchivos / request.TamañoPagina),
            TieneSiguiente = request.Pagina * request.TamañoPagina < totalArchivos,
            TieneAnterior = request.Pagina > 1
        };
    }

    public async Task<ArchivoUsuarioDto?> ObtenerPorIdAsync(int archivoId, int usuarioId)
    {
        var archivo = await _context.ArchivosUsuario
            .Include(a => a.Usuario)
            .FirstOrDefaultAsync(a => a.Id == archivoId && a.UsuarioId == usuarioId && a.Activo);

        return archivo == null ? null : await MapearArchivoUsuarioDto(archivo);
    }

    public async Task<ArchivoUsuarioDto?> ActualizarAsync(int archivoId, ActualizarArchivoUsuarioDto dto, int usuarioId, string modificadoPor)
    {
        var archivo = await _context.ArchivosUsuario
            .FirstOrDefaultAsync(a => a.Id == archivoId && a.UsuarioId == usuarioId && a.Activo);

        if (archivo == null) return null;

        // Actualizar campos si se proporcionan
        if (!string.IsNullOrWhiteSpace(dto.NombreArchivo))
            archivo.NombreArchivo = dto.NombreArchivo;

        if (dto.Descripcion != null)
            archivo.Descripcion = dto.Descripcion;

        if (dto.Tags != null)
            archivo.TagsMetadata = dto.Tags.Any() ? JsonConvert.SerializeObject(dto.Tags) : null;

        if (dto.EsFavorito.HasValue)
            archivo.EsFavorito = dto.EsFavorito.Value;

        archivo.FechaModificacion = DateTime.UtcNow;
        archivo.ModificadoPor = modificadoPor;

        await _context.SaveChangesAsync();

        return await MapearArchivoUsuarioDto(archivo);
    }

    public async Task<bool> EliminarAsync(int archivoId, int usuarioId)
    {
        var archivo = await _context.ArchivosUsuario
            .FirstOrDefaultAsync(a => a.Id == archivoId && a.UsuarioId == usuarioId);

        if (archivo == null) return false;

        // Soft delete
        archivo.Activo = false;
        archivo.FechaModificacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("🗑️ Archivo eliminado (soft delete): {ArchivoId}", archivoId);
        return true;
    }

    public async Task<bool> MarcarComoFavoritoAsync(int archivoId, int usuarioId, bool esFavorito)
    {
        var archivo = await _context.ArchivosUsuario
            .FirstOrDefaultAsync(a => a.Id == archivoId && a.UsuarioId == usuarioId && a.Activo);

        if (archivo == null) return false;

        archivo.EsFavorito = esFavorito;
        archivo.FechaModificacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<byte[]> DescargarArchivoAsync(int archivoId, int usuarioId)
    {
        var archivo = await _context.ArchivosUsuario
            .FirstOrDefaultAsync(a => a.Id == archivoId && a.UsuarioId == usuarioId && a.Activo);

        if (archivo == null || !File.Exists(archivo.RutaArchivo))
            throw new FileNotFoundException("Archivo no encontrado");

        // Registrar uso
        await RegistrarUsoAsync(archivoId, usuarioId);

        return await File.ReadAllBytesAsync(archivo.RutaArchivo);
    }

    public async Task<bool> RenombrarArchivoAsync(int archivoId, int usuarioId, string nuevoNombre)
    {
        var archivo = await _context.ArchivosUsuario
            .FirstOrDefaultAsync(a => a.Id == archivoId && a.UsuarioId == usuarioId && a.Activo);

        if (archivo == null) return false;

        archivo.NombreArchivo = nuevoNombre;
        archivo.FechaModificacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task RegistrarUsoAsync(int archivoId, int usuarioId)
    {
        var archivo = await _context.ArchivosUsuario
            .FirstOrDefaultAsync(a => a.Id == archivoId && a.UsuarioId == usuarioId && a.Activo);

        if (archivo != null)
        {
            archivo.VecesUtilizado++;
            archivo.UltimoAcceso = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<EstadisticasArchivosDto> ObtenerEstadisticasAsync(int usuarioId)
    {
        var archivos = await _context.ArchivosUsuario
            .Where(a => a.UsuarioId == usuarioId && a.Activo)
            .ToListAsync();

        var estadisticas = new EstadisticasArchivosDto
        {
            TotalArchivos = archivos.Count,
            EspacioUsadoBytes = archivos.Sum(a => a.TamañoBytes),
            EspacioUsadoFormateado = FormatearTamaño(archivos.Sum(a => a.TamañoBytes)),
            ArchivosPorTipo = archivos
                .GroupBy(a => a.TipoArchivo)
                .ToDictionary(g => g.Key.ToString(), g => g.Count())
        };

        var archivoMasUsado = archivos.OrderByDescending(a => a.VecesUtilizado).FirstOrDefault();
        if (archivoMasUsado != null)
            estadisticas.ArchivoMasUsado = await MapearArchivoUsuarioDto(archivoMasUsado);

        var archivoReciente = archivos.OrderByDescending(a => a.FechaSubida).FirstOrDefault();
        if (archivoReciente != null)
            estadisticas.ArchivoReciente = await MapearArchivoUsuarioDto(archivoReciente);

        return estadisticas;
    }

    public async Task<List<ArchivoUsuarioDto>> ObtenerFavoritosAsync(int usuarioId)
    {
        var archivos = await _context.ArchivosUsuario
            .Include(a => a.Usuario)
            .Where(a => a.UsuarioId == usuarioId && a.Activo && a.EsFavorito)
            .OrderByDescending(a => a.FechaSubida)
            .ToListAsync();

        var resultado = new List<ArchivoUsuarioDto>();
        foreach (var archivo in archivos)
        {
            resultado.Add(await MapearArchivoUsuarioDto(archivo));
        }

        return resultado;
    }

    public async Task<List<ArchivoUsuarioDto>> ObtenerRecientesAsync(int usuarioId, int cantidad = 5)
    {
        var archivos = await _context.ArchivosUsuario
            .Include(a => a.Usuario)
            .Where(a => a.UsuarioId == usuarioId && a.Activo)
            .OrderByDescending(a => a.FechaSubida)
            .Take(cantidad)
            .ToListAsync();

        var resultado = new List<ArchivoUsuarioDto>();
        foreach (var archivo in archivos)
        {
            resultado.Add(await MapearArchivoUsuarioDto(archivo));
        }

        return resultado;
    }

    public async Task<List<ArchivoUsuarioDto>> ObtenerPorTipoAsync(int usuarioId, TipoArchivo tipoArchivo)
    {
        var archivos = await _context.ArchivosUsuario
            .Include(a => a.Usuario)
            .Where(a => a.UsuarioId == usuarioId && a.Activo && a.TipoArchivo == tipoArchivo)
            .OrderByDescending(a => a.FechaSubida)
            .ToListAsync();

        var resultado = new List<ArchivoUsuarioDto>();
        foreach (var archivo in archivos)
        {
            resultado.Add(await MapearArchivoUsuarioDto(archivo));
        }

        return resultado;
    }

    public async Task<bool> ValidarPropietarioAsync(int archivoId, int usuarioId)
    {
        return await _context.ArchivosUsuario
            .AnyAsync(a => a.Id == archivoId && a.UsuarioId == usuarioId && a.Activo);
    }

    public async Task<bool> ExisteArchivoAsync(int archivoId)
    {
        return await _context.ArchivosUsuario
            .AnyAsync(a => a.Id == archivoId && a.Activo);
    }

    public async Task<long> ObtenerEspacioUsadoPorUsuarioAsync(int usuarioId)
    {
        return await _context.ArchivosUsuario
            .Where(a => a.UsuarioId == usuarioId && a.Activo)
            .SumAsync(a => a.TamañoBytes);
    }

    // Métodos privados de utilidad
    private async Task<ArchivoUsuarioDto> MapearArchivoUsuarioDto(ArchivoUsuario archivo)
    {
        return await Task.Run(() =>
        {
        var dto = _mapper.Map<ArchivoUsuarioDto>(archivo);
        dto.TipoArchivoTexto = ObtenerTextoTipoArchivo(archivo.TipoArchivo);
        dto.TamañoFormateado = FormatearTamaño(archivo.TamañoBytes);
        dto.UsuarioNombre = archivo.Usuario?.Username ?? "Usuario";
        
        if (!string.IsNullOrWhiteSpace(archivo.TagsMetadata))
        {
            try
            {
                dto.Tags = JsonConvert.DeserializeObject<List<string>>(archivo.TagsMetadata) ?? new List<string>();
            }
            catch
            {
                dto.Tags = new List<string>();
            }
        }

        return dto;
        });
    }

    private static bool EsExtensionPermitida(string nombreArchivo)
    {
        var extensionesPermitidas = new[] { ".xlsx" };
        var extension = Path.GetExtension(nombreArchivo).ToLower();
        return extensionesPermitidas.Contains(extension);
    }

    private static string ObtenerTextoTipoArchivo(TipoArchivo tipo)
    {
        return tipo switch
        {
            TipoArchivo.Ventas => "Ventas",
            TipoArchivo.ComprasCabecera => "Compras - Cabecera",
            TipoArchivo.ComprasDetalles => "Compras - Detalles",
            TipoArchivo.Stock => "Stock",
            TipoArchivo.Clientes => "Clientes",
            TipoArchivo.Productos => "Productos",
            TipoArchivo.General => "General",
            _ => tipo.ToString()
        };
    }

    private static string FormatearTamaño(long bytes)
    {
        string[] sufijos = { "B", "KB", "MB", "GB" };
        int contador = 0;
        decimal numero = bytes;
        
        while (Math.Round(numero / 1024) >= 1)
        {
            numero /= 1024;
            contador++;
        }
        
        return $"{numero:n1} {sufijos[contador]}";
    }
}