using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class EnvioService : IEnvioService
{
    private readonly NotizapDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<EnvioService> _logger;

    public EnvioService(NotizapDbContext context, IMapper mapper, ILogger<EnvioService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<EnvioDiarioDto>> ObtenerPorMesAsync(int year, int month)
    {
        var envios = await _context.EnviosDiarios
            .Where(e => e.Fecha.Year == year && e.Fecha.Month == month)
            .OrderBy(e => e.Fecha)
            .ToListAsync();

        return _mapper.Map<List<EnvioDiarioDto>>(envios);
    }

    public async Task<EnvioDiarioDto?> ObtenerPorFechaAsync(DateTime fecha)
    {
        var envio = await _context.EnviosDiarios
            .FirstOrDefaultAsync(e => e.Fecha.Date == fecha.Date);

        return envio == null ? null : _mapper.Map<EnvioDiarioDto>(envio);
    }

    public async Task CrearOActualizarAsync(CreateEnvioDiarioDto dto)
    {
        if (dto == null)
        {
            _logger.LogWarning("Intento de crear/actualizar envío con DTO nulo");
            throw new ArgumentNullException(nameof(dto));
        }

        var existente = await _context.EnviosDiarios
            .FirstOrDefaultAsync(e => e.Fecha.Date == dto.Fecha.Date);

        if (existente != null)
        {
            _logger.LogInformation("Actualizando envío existente para fecha {Fecha}", dto.Fecha);
            _mapper.Map(dto, existente);
        }
        else
        {
            _logger.LogInformation("Creando nuevo envío para fecha {Fecha}", dto.Fecha);
            var nuevo = _mapper.Map<EnvioDiario>(dto);
            _context.EnviosDiarios.Add(nuevo);
        }

        await _context.SaveChangesAsync();
    }

    public async Task EditarAsync(int id, CreateEnvioDiarioDto dto)
    {
        var envio = await _context.EnviosDiarios.FindAsync(id);
        if (envio == null)
        {
            _logger.LogWarning("No se encontró envío con Id {Id} para editar", id);
            throw new Exception("Envío no encontrado");
        }

        _logger.LogInformation("Editando envío Id {Id} para fecha {Fecha}", id, dto.Fecha);
        _mapper.Map(dto, envio);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(int id)
    {
        var envio = await _context.EnviosDiarios.FindAsync(id);
        if (envio == null)
        {
            _logger.LogWarning("No se encontró envío con Id {Id} para eliminar", id);
            throw new Exception("Envío no encontrado");
        }

        _logger.LogInformation("Eliminando envío Id {Id} para fecha {Fecha}", id, envio.Fecha);
        _context.EnviosDiarios.Remove(envio);
        await _context.SaveChangesAsync();
    }

    public async Task<EnvioResumenMensualDto> ObtenerResumenMensualAsync(int year, int month)
    {
        var envios = await _context.EnviosDiarios
            .Where(e => e.Fecha.Year == year && e.Fecha.Month == month)
            .ToListAsync();

        return new EnvioResumenMensualDto
        {
            TotalOca = envios.Sum(e => e.Oca ?? 0),
            TotalAndreani = envios.Sum(e => e.Andreani ?? 0),
            TotalRetirosSucursal = envios.Sum(e => e.RetirosSucursal ?? 0),
            TotalRoberto = envios.Sum(e => e.Roberto ?? 0),
            TotalTino = envios.Sum(e => e.Tino ?? 0),
            TotalCaddy = envios.Sum(e => e.Caddy ?? 0),
            TotalMercadoLibre = envios.Sum(e => e.MercadoLibre ?? 0),
        };
    }

    public async Task<ResultadoLoteDto> CrearOActualizarLoteAsync(List<CreateEnvioDiarioDto> envios)
    {
        var resultado = new ResultadoLoteDto();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var dto in envios)
            {
                try
                {
                    if (dto.Fecha == default)
                    {
                        resultado.Fallidos++;
                        resultado.Errores.Add($"Fecha inválida en uno de los registros");
                        _logger.LogWarning("Registro de lote con fecha inválida: {Dto}", dto);
                        continue;
                    }

                    var existente = await _context.EnviosDiarios
                        .FirstOrDefaultAsync(e => e.Fecha.Date == dto.Fecha.Date);

                    if (existente != null)
                    {
                        _logger.LogInformation("Actualizando registro existente en lote para fecha {Fecha}", dto.Fecha);

                        if (dto.Oca.HasValue) existente.Oca = dto.Oca.Value;
                        if (dto.Andreani.HasValue) existente.Andreani = dto.Andreani.Value;
                        if (dto.RetirosSucursal.HasValue) existente.RetirosSucursal = dto.RetirosSucursal.Value;
                        if (dto.Roberto.HasValue) existente.Roberto = dto.Roberto.Value;
                        if (dto.Tino.HasValue) existente.Tino = dto.Tino.Value;
                        if (dto.Caddy.HasValue) existente.Caddy = dto.Caddy.Value;
                        if (dto.MercadoLibre.HasValue) existente.MercadoLibre = dto.MercadoLibre.Value;
                    }
                    else
                    {
                        _logger.LogInformation("Creando nuevo registro en lote para fecha {Fecha}", dto.Fecha);

                        var nuevo = new EnvioDiario
                        {
                            Fecha = dto.Fecha,
                            Oca = dto.Oca,
                            Andreani = dto.Andreani,
                            RetirosSucursal = dto.RetirosSucursal,
                            Roberto = dto.Roberto,
                            Tino = dto.Tino,
                            Caddy = dto.Caddy,
                            MercadoLibre = dto.MercadoLibre
                        };

                        _context.EnviosDiarios.Add(nuevo);
                    }

                    resultado.Exitosos++;
                }
                catch (Exception ex)
                {
                    resultado.Fallidos++;
                    resultado.Errores.Add($"Error en fecha {dto.Fecha:dd/MM/yyyy}: {ex.Message}");
                    _logger.LogError(ex, "Error procesando registro de lote para fecha {Fecha}", dto.Fecha);
                }
            }

            // Solo commit si NO hay errores
            if (resultado.Fallidos == 0)
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                resultado.Mensaje = $"✅ Todos los registros guardados correctamente ({resultado.Exitosos} envíos)";
            }
            else
            {
                await transaction.RollbackAsync();
                resultado.Mensaje = $"❌ Error: {resultado.Fallidos} registros fallaron.";
                resultado.Exitosos = 0;
                _logger.LogWarning("Rollback de lote por {Fallidos} fallidos", resultado.Fallidos);
            }

            return resultado;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error crítico al procesar lote de envíos");
            throw new Exception($"Error crítico al procesar lote: {ex.Message}");
        }
    }
}
