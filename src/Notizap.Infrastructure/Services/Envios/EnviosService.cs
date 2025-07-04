using AutoMapper;
using Microsoft.EntityFrameworkCore;

public class EnvioService : IEnvioService
{
    private readonly NotizapDbContext _context;
    private readonly IMapper _mapper;

    public EnvioService(NotizapDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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
        var existente = await _context.EnviosDiarios
            .FirstOrDefaultAsync(e => e.Fecha.Date == dto.Fecha.Date);

        if (existente != null)
        {
            _mapper.Map(dto, existente);
        }
        else
        {
            var nuevo = _mapper.Map<EnvioDiario>(dto);
            _context.EnviosDiarios.Add(nuevo);
        }

        await _context.SaveChangesAsync();
    }

    public async Task EditarAsync(int id, CreateEnvioDiarioDto dto)
    {
        var envio = await _context.EnviosDiarios.FindAsync(id);
        if (envio == null) throw new Exception("Envío no encontrado");

        _mapper.Map(dto, envio);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(int id)
    {
        var envio = await _context.EnviosDiarios.FindAsync(id);
        if (envio == null) throw new Exception("Envío no encontrado");

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
            TotalOca = envios.Sum(e => e.Oca),
            TotalAndreani = envios.Sum(e => e.Andreani),
            TotalRetirosSucursal = envios.Sum(e => e.RetirosSucursal),
            TotalRoberto = envios.Sum(e => e.Roberto),
            TotalTino = envios.Sum(e => e.Tino),
            TotalCaddy = envios.Sum(e => e.Caddy),
            TotalMercadoLibre = envios.Sum(e => e.MercadoLibre),
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
                    // Validar DTO individualmente
                    if (dto.Fecha == default)
                    {
                        resultado.Fallidos++;
                        resultado.Errores.Add($"Fecha inválida en uno de los registros");
                        continue;
                    }

                    // Buscar registro existente por fecha
                    var existente = await _context.EnviosDiarios
                        .FirstOrDefaultAsync(e => e.Fecha.Date == dto.Fecha.Date);

                    if (existente != null)
                    {
                        // Actualizar registro existente
                        _mapper.Map(dto, existente);
                    }
                    else
                    {
                        // Crear nuevo registro
                        var nuevo = _mapper.Map<EnvioDiario>(dto);
                        _context.EnviosDiarios.Add(nuevo);
                    }
                    
                    resultado.Exitosos++;
                }
                catch (Exception ex)
                {
                    resultado.Fallidos++;
                    resultado.Errores.Add($"Error en fecha {dto.Fecha:dd/MM/yyyy}: {ex.Message}");
                }
            }

            // Solo hacer commit si NO hay errores
            if (resultado.Fallidos == 0)
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                resultado.Mensaje = $"✅ Todos los registros guardados correctamente ({resultado.Exitosos} envíos)";
            }
            else
            {
                await transaction.RollbackAsync();
                resultado.Mensaje = $"❌ Error: {resultado.Fallidos} registros fallaron. Intenta guardando celda por celda para encontrar el error específico.";
                resultado.Exitosos = 0; // Reset porque hicimos rollback
            }
            
            return resultado;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"Error crítico al procesar lote: {ex.Message}");
        }
    }
}
