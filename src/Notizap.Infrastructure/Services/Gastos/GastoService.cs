using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Notizap.Infrastructure.Services
{
    public class GastoService : IGastoService
    {
        private readonly NotizapDbContext _context;
        private readonly IMapper _mapper;

        public GastoService(NotizapDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        #region Métodos Existentes

        public async Task<IEnumerable<GastoDto>> ObtenerTodosAsync()
        {
            var gastos = await _context.Gastos.OrderByDescending(g => g.Fecha).ToListAsync();
            return _mapper.Map<IEnumerable<GastoDto>>(gastos);
        }

        public async Task<GastoDto?> ObtenerPorIdAsync(int id)
        {
            var gasto = await _context.Gastos.FindAsync(id);
            return gasto is null ? null : _mapper.Map<GastoDto>(gasto);
        }

        public async Task<GastoDto> CrearAsync(CreateGastoDto dto)
        {
            var gasto = _mapper.Map<Gasto>(dto);
            
            // Si no se especifica fecha, usar la fecha actual EN UTC
            if (dto.Fecha == null)
            {
                gasto.Fecha = DateTime.UtcNow;
            }
            else
            {
                // Asegurar que la fecha esté en UTC
                gasto.Fecha = dto.Fecha.Value.Kind == DateTimeKind.Utc 
                    ? dto.Fecha.Value 
                    : dto.Fecha.Value.ToUniversalTime();
            }
            
            gasto.FechaCreacion = DateTime.UtcNow;
            
            _context.Gastos.Add(gasto);
            await _context.SaveChangesAsync();
            return _mapper.Map<GastoDto>(gasto);
        }

        public async Task<GastoDto?> ActualizarAsync(int id, UpdateGastoDto dto)
        {
            var gasto = await _context.Gastos.FindAsync(id);
            if (gasto is null) return null;

            _mapper.Map(dto, gasto);
            
            gasto.Fecha = dto.Fecha.Kind == DateTimeKind.Utc 
                ? dto.Fecha 
                : dto.Fecha.ToUniversalTime();
            
            await _context.SaveChangesAsync();
            return _mapper.Map<GastoDto>(gasto);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var gasto = await _context.Gastos.FindAsync(id);
            if (gasto is null) return false;

            _context.Gastos.Remove(gasto);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Métodos Avanzados

        public async Task<(IEnumerable<GastoDto> gastos, int totalCount)> ObtenerConFiltrosAsync(GastoFiltrosDto filtros)
        {
            var query = _context.Gastos.AsQueryable();

            if (filtros.FechaDesde.HasValue)
            {
                var fechaDesdeUtc = filtros.FechaDesde.Value.Kind == DateTimeKind.Utc 
                    ? filtros.FechaDesde.Value 
                    : filtros.FechaDesde.Value.ToUniversalTime();
                query = query.Where(g => g.Fecha >= fechaDesdeUtc);
            }

            if (filtros.FechaHasta.HasValue)
            {
                var fechaHastaUtc = filtros.FechaHasta.Value.Kind == DateTimeKind.Utc 
                    ? filtros.FechaHasta.Value 
                    : filtros.FechaHasta.Value.ToUniversalTime();
                query = query.Where(g => g.Fecha <= fechaHastaUtc);
            }

            if (!string.IsNullOrEmpty(filtros.Categoria))
            {
                query = query.Where(g => g.Categoria == filtros.Categoria);
            }

            if (filtros.MontoMinimo.HasValue)
            {
                query = query.Where(g => g.Monto >= filtros.MontoMinimo.Value);
            }

            if (filtros.MontoMaximo.HasValue)
            {
                query = query.Where(g => g.Monto <= filtros.MontoMaximo.Value);
            }

            if (!string.IsNullOrEmpty(filtros.Busqueda))
            {
                query = query.Where(g => 
                    g.Nombre.Contains(filtros.Busqueda) || 
                    (g.Descripcion != null && g.Descripcion.Contains(filtros.Busqueda)) ||
                    (g.Proveedor != null && g.Proveedor.Contains(filtros.Busqueda))
                );
            }

            if (filtros.EsImportante.HasValue)
            {
                query = query.Where(g => g.EsImportante == filtros.EsImportante.Value);
            }

            if (filtros.EsRecurrente.HasValue)
            {
                query = query.Where(g => g.EsRecurrente == filtros.EsRecurrente.Value);
            }

            // Contar total antes de paginar
            var totalCount = await query.CountAsync();

            // Aplicar ordenamiento
            query = filtros.OrdenarPor?.ToLower() switch
            {
                "monto" => filtros.Descendente ? query.OrderByDescending(g => g.Monto) : query.OrderBy(g => g.Monto),
                "nombre" => filtros.Descendente ? query.OrderByDescending(g => g.Nombre) : query.OrderBy(g => g.Nombre),
                "categoria" => filtros.Descendente ? query.OrderByDescending(g => g.Categoria) : query.OrderBy(g => g.Categoria),
                _ => filtros.Descendente ? query.OrderByDescending(g => g.Fecha) : query.OrderBy(g => g.Fecha)
            };

            // Aplicar paginación
            var gastos = await query
                .Skip((filtros.Pagina - 1) * filtros.TamañoPagina)
                .Take(filtros.TamañoPagina)
                .ToListAsync();

            return (_mapper.Map<IEnumerable<GastoDto>>(gastos), totalCount);
        }

        public async Task<GastoResumenDto> ObtenerResumenMensualAsync(int año, int mes)
        {
            var fechaInicioMes = new DateTime(año, mes, 1, 0, 0, 0, DateTimeKind.Utc);
            var fechaFinMes = fechaInicioMes.AddMonths(1).AddTicks(-1);

            var fechaInicioMesAnterior = fechaInicioMes.AddMonths(-1);
            var fechaFinMesAnterior = fechaInicioMesAnterior.AddMonths(1).AddTicks(-1);

            // Gastos del mes actual
            var gastosActuales = await _context.Gastos
                .Where(g => g.Fecha >= fechaInicioMes && g.Fecha <= fechaFinMes)
                .ToListAsync();

            // Gastos del mes anterior
            var gastosAnteriores = await _context.Gastos
                .Where(g => g.Fecha >= fechaInicioMesAnterior && g.Fecha <= fechaFinMesAnterior)
                .ToListAsync();

            var totalMes = gastosActuales.Sum(g => g.Monto);
            var totalMesAnterior = gastosAnteriores.Sum(g => g.Monto);

            var porcentajeCambio = totalMesAnterior != 0 
                ? ((totalMes - totalMesAnterior) / totalMesAnterior) * 100 
                : 0;

            // Categoría más gastada
            var categoriaMasGastada = gastosActuales
                .GroupBy(g => g.Categoria)
                .OrderByDescending(g => g.Sum(x => x.Monto))
                .FirstOrDefault();

            // Promedio mensual (últimos 12 meses)
            var hace12Meses = fechaInicioMes.AddMonths(-12);
            var gastosUltimos12Meses = await _context.Gastos
                .Where(g => g.Fecha >= hace12Meses)
                .ToListAsync(); 

            var promedioMensual = gastosUltimos12Meses
                .GroupBy(g => new { g.Fecha.Year, g.Fecha.Month })
                .Select(grupo => grupo.Sum(g => g.Monto))
                .DefaultIfEmpty(0)
                .Average();

            return new GastoResumenDto
            {
                TotalMes = totalMes,
                TotalMesAnterior = totalMesAnterior,
                PorcentajeCambio = porcentajeCambio,
                CategoriaMasGastada = categoriaMasGastada?.Key ?? "Sin categoría",
                MontoCategoriaMasGastada = categoriaMasGastada?.Sum(g => g.Monto) ?? 0,
                PromedioMensual = promedioMensual,
                CantidadGastos = gastosActuales.Count
            };
        }

        public async Task<IEnumerable<GastoPorCategoriaDto>> ObtenerGastosPorCategoriaAsync(DateTime? desde = null, DateTime? hasta = null)
        {
            var query = _context.Gastos.AsQueryable();

            if (desde.HasValue)
            {
                var desdeUtc = desde.Value.Kind == DateTimeKind.Utc 
                    ? desde.Value 
                    : desde.Value.ToUniversalTime();
                query = query.Where(g => g.Fecha >= desdeUtc);
            }

            if (hasta.HasValue)
            {
                var hastaUtc = hasta.Value.Kind == DateTimeKind.Utc 
                    ? hasta.Value 
                    : hasta.Value.ToUniversalTime();
                query = query.Where(g => g.Fecha <= hastaUtc);
            }

            var gastosPorCategoria = await query
                .GroupBy(g => g.Categoria)
                .Select(g => new
                {
                    Categoria = g.Key,
                    TotalMonto = g.Sum(x => x.Monto),
                    CantidadGastos = g.Count()
                })
                .OrderByDescending(g => g.TotalMonto)
                .ToListAsync();

            var totalGeneral = gastosPorCategoria.Sum(g => g.TotalMonto);

            return gastosPorCategoria.Select(g => new GastoPorCategoriaDto
            {
                Categoria = g.Categoria,
                TotalMonto = g.TotalMonto,
                CantidadGastos = g.CantidadGastos,
                Porcentaje = totalGeneral > 0 ? (g.TotalMonto / totalGeneral) * 100 : 0
            });
        }

        public async Task<IEnumerable<string>> ObtenerCategoriasAsync()
        {
            return await _context.Gastos
                .Select(g => g.Categoria)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<IEnumerable<GastoTendenciaDto>> ObtenerTendenciaMensualAsync(int meses = 12)
        {
            var fechaLimite = DateTime.UtcNow.AddMonths(-meses);

            var tendencia = await _context.Gastos
                .Where(g => g.Fecha >= fechaLimite)
                .GroupBy(g => new { g.Fecha.Year, g.Fecha.Month })
                .Select(g => new
                {
                    Año = g.Key.Year,
                    Mes = g.Key.Month,
                    TotalMonto = g.Sum(x => x.Monto),
                    CantidadGastos = g.Count()
                })
                .OrderBy(g => g.Año)
                .ThenBy(g => g.Mes)
                .ToListAsync();

            var cultura = new CultureInfo("es-ES");
            
            return tendencia.Select(t => new GastoTendenciaDto
            {
                Año = t.Año,
                Mes = t.Mes,
                MesNombre = cultura.DateTimeFormat.GetMonthName(t.Mes),
                TotalMonto = t.TotalMonto,
                CantidadGastos = t.CantidadGastos,
                PromedioGasto = t.CantidadGastos > 0 ? t.TotalMonto / t.CantidadGastos : 0
            });
        }

        public async Task<IEnumerable<GastoDto>> ObtenerGastosRecurrentesAsync()
        {
            var gastos = await _context.Gastos
                .Where(g => g.EsRecurrente)
                .OrderByDescending(g => g.Fecha)
                .ToListAsync();

            return _mapper.Map<IEnumerable<GastoDto>>(gastos);
        }

        public async Task<IEnumerable<GastoDto>> ObtenerTopGastosAsync(int cantidad = 5, DateTime? desde = null, DateTime? hasta = null)
        {
            var query = _context.Gastos.AsQueryable();

            if (desde.HasValue)
            {
                var desdeUtc = desde.Value.Kind == DateTimeKind.Utc 
                    ? desde.Value 
                    : desde.Value.ToUniversalTime();
                query = query.Where(g => g.Fecha >= desdeUtc);
            }

            if (hasta.HasValue)
            {
                var hastaUtc = hasta.Value.Kind == DateTimeKind.Utc 
                    ? hasta.Value 
                    : hasta.Value.ToUniversalTime();
                query = query.Where(g => g.Fecha <= hastaUtc);
            }

            var gastos = await query
                .OrderByDescending(g => g.Monto)
                .Take(cantidad)
                .ToListAsync();

            return _mapper.Map<IEnumerable<GastoDto>>(gastos);
        }

        #endregion
    }
}