using Microsoft.EntityFrameworkCore;
using System.Globalization;
public class DevolucionMercadoLibreService : IDevolucionMercadoLibreService
{
    private readonly NotizapDbContext _context;

    public DevolucionMercadoLibreService(NotizapDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DevolucionMercadoLibreDto>> GetAllAsync()
    {
        var devoluciones = await _context.DevolucionesMercadoLibre
            .OrderByDescending(d => d.Fecha)
            .ToListAsync();

        return devoluciones.Select(MapToDto);
    }

    public async Task<DevolucionMercadoLibreDto?> GetByIdAsync(int id)
    {
        var devolucion = await _context.DevolucionesMercadoLibre
            .FirstOrDefaultAsync(d => d.Id == id);

        return devolucion != null ? MapToDto(devolucion) : null;
    }

    public async Task<DevolucionMercadoLibreDto> CreateAsync(CreateDevolucionMercadoLibreDto dto)
    {
        var devolucion = new DevolucionMercadoLibre
        {
            Fecha = dto.Fecha,
            Cliente = dto.Cliente.Trim(),
            Modelo = dto.Modelo.Trim(),
            NotaCreditoEmitida = dto.NotaCreditoEmitida,
            Pedido = dto.Pedido.Trim(),
            FechaCreacion = DateTime.UtcNow
        };

        _context.DevolucionesMercadoLibre.Add(devolucion);
        await _context.SaveChangesAsync();

        return MapToDto(devolucion);
    }

    public async Task<DevolucionMercadoLibreDto?> UpdateAsync(int id, UpdateDevolucionMercadoLibreDto dto)
    {
        var devolucion = await _context.DevolucionesMercadoLibre
            .FirstOrDefaultAsync(d => d.Id == id);

        if (devolucion == null)
            return null;

        devolucion.Fecha = dto.Fecha;
        devolucion.Cliente = dto.Cliente.Trim();
        devolucion.Modelo = dto.Modelo.Trim();
        devolucion.NotaCreditoEmitida = dto.NotaCreditoEmitida;
        devolucion.FechaActualizacion = DateTime.UtcNow;
        devolucion.Pedido = dto.Pedido.Trim();

        await _context.SaveChangesAsync();

        return MapToDto(devolucion);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var devolucion = await _context.DevolucionesMercadoLibre
            .FirstOrDefaultAsync(d => d.Id == id);

        if (devolucion == null)
            return false;

        _context.DevolucionesMercadoLibre.Remove(devolucion);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<DevolucionMercadoLibreDto>> GetFilteredAsync(DevolucionMercadoLibreFiltrosDto filtros)
    {
        var query = _context.DevolucionesMercadoLibre.AsQueryable();

        // Filtro por año
        if (filtros.Año.HasValue)
        {
            query = query.Where(d => d.Fecha.Year == filtros.Año.Value);
        }

        // Filtro por mes
        if (filtros.Mes.HasValue)
        {
            query = query.Where(d => d.Fecha.Month == filtros.Mes.Value);
        }

        // Filtro por cliente
        if (!string.IsNullOrWhiteSpace(filtros.Cliente))
        {
            query = query.Where(d => d.Cliente.ToLower().Contains(filtros.Cliente.ToLower()));
        }

        // Filtro por modelo
        if (!string.IsNullOrWhiteSpace(filtros.Modelo))
        {
            query = query.Where(d => d.Modelo.ToLower().Contains(filtros.Modelo.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(filtros.Pedido))
        {
            query = query.Where(d => d.Pedido.ToLower().Contains(filtros.Pedido.ToLower()));
        }

        // Filtro por nota de crédito
        if (filtros.NotaCreditoEmitida.HasValue)
        {
            query = query.Where(d => d.NotaCreditoEmitida == filtros.NotaCreditoEmitida.Value);
        }

        var devoluciones = await query
            .OrderByDescending(d => d.Fecha)
            .ToListAsync();

        return devoluciones.Select(MapToDto);
    }

    public async Task<bool> UpdateNotaCreditoAsync(int id, bool notaCreditoEmitida)
    {
        var devolucion = await _context.DevolucionesMercadoLibre
            .FirstOrDefaultAsync(d => d.Id == id);

        if (devolucion == null)
            return false;

        devolucion.NotaCreditoEmitida = notaCreditoEmitida;
        devolucion.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<DevolucionMercadoLibreEstadisticasDto> GetEstadisticasAsync()
    {
        var todasLasDevoluciones = await _context.DevolucionesMercadoLibre.ToListAsync();
        return CalcularEstadisticas(todasLasDevoluciones);
    }

    public async Task<DevolucionMercadoLibreEstadisticasDto> GetEstadisticasFilteredAsync(DevolucionMercadoLibreFiltrosDto filtros)
    {
        var devolucionesFiltradas = await GetFilteredAsync(filtros);
        var devolucionesEntity = devolucionesFiltradas.Select(dto => new DevolucionMercadoLibre
        {
            Id = dto.Id,
            Fecha = dto.Fecha,
            Cliente = dto.Cliente,
            Modelo = dto.Modelo,
            NotaCreditoEmitida = dto.NotaCreditoEmitida,
            FechaCreacion = dto.FechaCreacion,
            FechaActualizacion = dto.FechaActualizacion
        }).ToList();

        return CalcularEstadisticas(devolucionesEntity);
    }

    public async Task<IEnumerable<(int Año, int Mes, string NombreMes)>> GetMesesDisponiblesAsync()
    {
        var fechas = await _context.DevolucionesMercadoLibre
            .Select(d => new { d.Fecha.Year, d.Fecha.Month })
            .Distinct()
            .OrderByDescending(f => f.Year)
            .ThenByDescending(f => f.Month)
            .ToListAsync();

        var culture = new CultureInfo("es-ES");
        return fechas.Select(f => (
            f.Year,
            f.Month,
            culture.DateTimeFormat.GetMonthName(f.Month)
        ));
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.DevolucionesMercadoLibre
            .AnyAsync(d => d.Id == id);
    }

    #region Métodos Privados

    private static DevolucionMercadoLibreDto MapToDto(DevolucionMercadoLibre entity)
    {
        return new DevolucionMercadoLibreDto
        {
            Id = entity.Id,
            Fecha = entity.Fecha,
            Cliente = entity.Cliente,
            Modelo = entity.Modelo,
            NotaCreditoEmitida = entity.NotaCreditoEmitida,
            FechaCreacion = entity.FechaCreacion,
            FechaActualizacion = entity.FechaActualizacion,
            Pedido = entity.Pedido
        };
    }

    private static DevolucionMercadoLibreEstadisticasDto CalcularEstadisticas(List<DevolucionMercadoLibre> devoluciones)
    {
        var total = devoluciones.Count;
        var notasEmitidas = devoluciones.Count(d => d.NotaCreditoEmitida);
        var notasPendientes = total - notasEmitidas;

        var fechaActual = DateTime.Now;
        var devolucionesMesActual = devoluciones.Count(d => 
            d.Fecha.Year == fechaActual.Year && 
            d.Fecha.Month == fechaActual.Month);

        var porcentajeNotas = total > 0 ? (decimal)notasEmitidas / total * 100 : 0;

        // Estadísticas por mes
        var estadisticasPorMes = devoluciones
            .GroupBy(d => new { d.Fecha.Year, d.Fecha.Month })
            .Select(g => new EstadisticasMensualDto
            {
                Año = g.Key.Year,
                Mes = g.Key.Month,
                NombreMes = new CultureInfo("es-ES").DateTimeFormat.GetMonthName(g.Key.Month),
                TotalDevoluciones = g.Count(),
                NotasCreditoEmitidas = g.Count(d => d.NotaCreditoEmitida),
                NotasCreditoPendientes = g.Count(d => !d.NotaCreditoEmitida)
            })
            .OrderByDescending(e => e.Año)
            .ThenByDescending(e => e.Mes)
            .ToList();

        return new DevolucionMercadoLibreEstadisticasDto
        {
            TotalDevoluciones = total,
            NotasCreditoEmitidas = notasEmitidas,
            NotasCreditoPendientes = notasPendientes,
            DevolucionesMesActual = devolucionesMesActual,
            PorcentajeNotasEmitidas = Math.Round(porcentajeNotas, 1),
            EstadisticasPorMes = estadisticasPorMes
        };
    }

    #endregion
}