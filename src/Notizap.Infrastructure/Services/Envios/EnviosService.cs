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
                        Console.WriteLine($"🔍 REGISTRO EXISTENTE encontrado:");
                        Console.WriteLine($"   Oca ANTES: {existente.Oca}");
                        Console.WriteLine($"   Andreani ANTES: {existente.Andreani}");
                        Console.WriteLine($"   Roberto ANTES: {existente.Roberto}");

                        if (dto.Oca.HasValue) existente.Oca = dto.Oca.Value;
                        if (dto.Andreani.HasValue) existente.Andreani = dto.Andreani.Value;
                        if (dto.RetirosSucursal.HasValue) existente.RetirosSucursal = dto.RetirosSucursal.Value;
                        if (dto.Roberto.HasValue) existente.Roberto = dto.Roberto.Value;
                        if (dto.Tino.HasValue) existente.Tino = dto.Tino.Value;
                        if (dto.Caddy.HasValue) existente.Caddy = dto.Caddy.Value;
                        if (dto.MercadoLibre.HasValue) existente.MercadoLibre = dto.MercadoLibre.Value;

                        Console.WriteLine($"🔍 REGISTRO DESPUÉS del update:");
                        Console.WriteLine($"   Oca DESPUÉS: {existente.Oca}");
                        Console.WriteLine($"   Andreani DESPUÉS: {existente.Andreani}");
                        Console.WriteLine($"   Roberto DESPUÉS: {existente.Roberto}");
                    }
                    else
                    {
                        Console.WriteLine($"🔍 CREANDO NUEVO REGISTRO");
                        
                        var nuevo = new EnvioDiario
                        {
                            Fecha = dto.Fecha,
                            Oca = dto.Oca.HasValue ? dto.Oca.Value : (int?)null,
                            Andreani = dto.Andreani.HasValue ? dto.Andreani.Value : (int?)null,
                            RetirosSucursal = dto.RetirosSucursal.HasValue ? dto.RetirosSucursal.Value : (int?)null,
                            Roberto = dto.Roberto.HasValue ? dto.Roberto.Value : (int?)null,
                            Tino = dto.Tino.HasValue ? dto.Tino.Value : (int?)null,
                            Caddy = dto.Caddy.HasValue ? dto.Caddy.Value : (int?)null,
                            MercadoLibre = dto.MercadoLibre.HasValue ? dto.MercadoLibre.Value : (int?)null
                        };
                        
                        _context.EnviosDiarios.Add(nuevo);
                        
                        Console.WriteLine($"🔍 NUEVO REGISTRO creado:");
                        Console.WriteLine($"   Oca: {nuevo.Oca} ({(nuevo.Oca.HasValue ? "valor asignado" : "null = no disponible")})");
                        Console.WriteLine($"   Andreani: {nuevo.Andreani} ({(nuevo.Andreani.HasValue ? "valor asignado" : "null = no disponible")})");
                        Console.WriteLine($"   Roberto: {nuevo.Roberto} ({(nuevo.Roberto.HasValue ? "valor asignado" : "null = no disponible")})");
                    }
                    
                    Console.WriteLine($"🔍 === FIN PROCESAMIENTO ===");
                    resultado.Exitosos++;
                }
                catch (Exception ex)
                {
                    resultado.Fallidos++;
                    resultado.Errores.Add($"Error en fecha {dto.Fecha:dd/MM/yyyy}: {ex.Message}");
                    Console.WriteLine($"🔍 ERROR: {ex.Message}");
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
                resultado.Mensaje = $"❌ Error: {resultado.Fallidos} registros fallaron.";
                resultado.Exitosos = 0;
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
