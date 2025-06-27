using Microsoft.EntityFrameworkCore;
public class VentaVendedoraService : IVentaVendedoraService
{
    private readonly NotizapDbContext _context;
    private readonly ExcelVentasProcessor _excelProcessor;

    public VentaVendedoraService(NotizapDbContext context)
    {
        _context = context;
        _excelProcessor = new ExcelVentasProcessor();
    }

    public async Task<(bool Success, string Message, VentaVendedoraStatsDto? Stats)> SubirArchivoVentasAsync(
        Stream archivoStream, bool sobreescribirDuplicados = false)
    {
        var resultado = await _excelProcessor.ProcesarArchivoAsync(archivoStream);
        
        if (!resultado.Success)
        {
            return (false, resultado.Message, null);
        }

        // Verificar duplicados por fecha si no se permite sobreescribir
        if (!sobreescribirDuplicados && resultado.FechaMinima.HasValue && resultado.FechaMaxima.HasValue)
        {
            var fechasExistentes = await _context.VentasVendedoras
                .Where(v => v.Fecha.Date >= resultado.FechaMinima.Value.Date && 
                            v.Fecha.Date <= resultado.FechaMaxima.Value.Date)
                .Select(v => v.Fecha.Date)
                .Distinct()
                .ToListAsync();

            if (fechasExistentes.Any())
            {
                var fechasTexto = string.Join(", ", fechasExistentes.Select(f => f.ToString("dd/MM/yyyy")));
                return (false, $"Ya existen datos para las fechas: {fechasTexto}. Use la opción de sobreescribir si desea reemplazarlos.", null);
            }
        }

        // Si se permite sobreescribir, eliminar datos existentes en el rango
        if (sobreescribirDuplicados && resultado.FechaMinima.HasValue && resultado.FechaMaxima.HasValue)
        {
            var ventasExistentes = await _context.VentasVendedoras
                .Where(v => v.Fecha.Date >= resultado.FechaMinima.Value.Date && 
                            v.Fecha.Date <= resultado.FechaMaxima.Value.Date)
                .ToListAsync();

            _context.VentasVendedoras.RemoveRange(ventasExistentes);
        }

        // Procesar y guardar las nuevas ventas
        var ventasParaGuardar = await ProcesarVentasParaGuardarAsync(resultado.Ventas);
        
        await _context.VentasVendedoras.AddRangeAsync(ventasParaGuardar);
        await _context.SaveChangesAsync();

        // Generar estadísticas del upload
        var stats = await GenerarEstadisticasAsync(new VentaVendedoraFilterDto 
        { 
            FechaInicio = resultado.FechaMinima,
            FechaFin = resultado.FechaMaxima
        });

        var mensaje = $"Se cargaron {ventasParaGuardar.Count} ventas correctamente. " +
                        $"Rango de fechas: {resultado.FechaMinima:dd/MM/yyyy} - {resultado.FechaMaxima:dd/MM/yyyy}";

        if (resultado.Errores.Any())
        {
            mensaje += $" Se encontraron {resultado.Errores.Count} errores durante el procesamiento.";
        }

        return (true, mensaje, stats);
    }

    private async Task<List<VentaVendedora>> ProcesarVentasParaGuardarAsync(List<VentaVendedoraCreateDto> ventasDto)
    {
        var ventasParaGuardar = new List<VentaVendedora>();

        // Obtener todas las sucursales y vendedores únicos del archivo
        var sucursalesNombres = ventasDto.Select(v => v.SucursalNombre).Distinct().ToList();
        var vendedoresNombres = ventasDto.Select(v => v.VendedorNombre).Distinct().ToList();

        // Crear o buscar sucursales
        var sucursalesDict = await ObtenerOCrearSucursalesAsync(sucursalesNombres);
        
        // Crear o buscar vendedores
        var vendedoresDict = await ObtenerOCrearVendedoresAsync(vendedoresNombres);

        foreach (var ventaDto in ventasDto)
        {
            var venta = new VentaVendedora
            {
                SucursalId = sucursalesDict[ventaDto.SucursalNombre],
                VendedorId = vendedoresDict[ventaDto.VendedorNombre],
                Producto = ventaDto.Producto,
                Fecha = ventaDto.Fecha,
                Cantidad = ventaDto.Cantidad,
                Total = ventaDto.Total,
                Turno = VentaVendedora.DeterminarTurno(ventaDto.Fecha),
                EsProductoDescuento = VentaVendedora.EsProductoEspecial(ventaDto.Producto),
                FechaCreacion = DateTime.UtcNow
            };

            ventasParaGuardar.Add(venta);
        }

        return ventasParaGuardar;
    }

    private async Task<Dictionary<string, int>> ObtenerOCrearSucursalesAsync(List<string> nombres)
    {
        var sucursalesExistentes = await _context.SucursalesVentas
            .Where(s => nombres.Contains(s.Nombre))
            .ToDictionaryAsync(s => s.Nombre, s => s.Id);

        var sucursalesNuevas = nombres
            .Where(nombre => !sucursalesExistentes.ContainsKey(nombre))
            .Select(nombre => new SucursalVenta 
            { 
                Nombre = nombre,
                AbreSabadoTarde = !SucursalVenta.SucursalesConHorarioEspecial().Contains(nombre),
                FechaCreacion = DateTime.UtcNow
            })
            .ToList();

        if (sucursalesNuevas.Any())
        {
            await _context.SucursalesVentas.AddRangeAsync(sucursalesNuevas);
            await _context.SaveChangesAsync();

            foreach (var sucursal in sucursalesNuevas)
            {
                sucursalesExistentes[sucursal.Nombre] = sucursal.Id;
            }
        }

        return sucursalesExistentes;
    }

    private async Task<Dictionary<string, int>> ObtenerOCrearVendedoresAsync(List<string> nombres)
    {
        var vendedoresExistentes = await _context.VendedoresVentas
            .Where(v => nombres.Contains(v.Nombre))
            .ToDictionaryAsync(v => v.Nombre, v => v.Id);

        var vendedoresNuevos = nombres
            .Where(nombre => !vendedoresExistentes.ContainsKey(nombre))
            .Select(nombre => new VendedorVenta 
            { 
                Nombre = nombre,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            })
            .ToList();

        if (vendedoresNuevos.Any())
        {
            await _context.VendedoresVentas.AddRangeAsync(vendedoresNuevos);
            await _context.SaveChangesAsync();

            foreach (var vendedor in vendedoresNuevos)
            {
                vendedoresExistentes[vendedor.Nombre] = vendedor.Id;
            }
        }

        return vendedoresExistentes;
    }

    private async Task<VentaVendedoraStatsDto> GenerarEstadisticasAsync(VentaVendedoraFilterDto filtros)
    {
        // Implementación básica de estadísticas
        var query = _context.VentasVendedoras
            .Include(v => v.Sucursal)
            .Include(v => v.Vendedor)
            .AsQueryable();

        if (filtros.FechaInicio.HasValue)
            query = query.Where(v => v.Fecha >= filtros.FechaInicio.Value);

        if (filtros.FechaFin.HasValue)
            query = query.Where(v => v.Fecha <= filtros.FechaFin.Value);

        var ventas = await query.ToListAsync();

        return new VentaVendedoraStatsDto
        {
            TotalVentas = ventas.Count,
            MontoTotal = ventas.Sum(v =>
                v.Cantidad < 0
                ? -v.Total  
                : v.Total),
            CantidadTotal = ventas.Sum(v => v.EsProductoDescuento && v.Cantidad == -1 ? 0 : v.Cantidad),
            DiasConVentas = ventas.GroupBy(v => v.Fecha.Date).Count()
        };
    }
    
    public async Task<(List<VentaVendedoraDto> Ventas, int TotalRegistros)> ObtenerVentasAsync(
        VentaVendedoraFilterDto filtros)
    {
        var query = _context.VentasVendedoras
            .Include(v => v.Sucursal)
            .Include(v => v.Vendedor)
            .AsQueryable();

        // Aplicar filtros
        query = AplicarFiltros(query, filtros);

        // Contar total antes de paginar
        var totalRegistros = await query.CountAsync();

        // Aplicar ordenamiento
        query = AplicarOrdenamiento(query, filtros.OrderBy, filtros.OrderDesc);

        // Aplicar paginación
        query = query.Skip((filtros.Page - 1) * filtros.PageSize)
                    .Take(filtros.PageSize);

        var ventas = await query.ToListAsync();

        var ventasDto = ventas.Select(v => new VentaVendedoraDto
        {
            Id = v.Id,
            SucursalNombre = v.Sucursal.Nombre,
            VendedorNombre = v.Vendedor.Nombre,
            Producto = v.Producto,
            Fecha = v.Fecha,
            Cantidad = v.Cantidad,
            CantidadReal = v.GetCantidadReal(),
            Total = v.Total,
            Turno = v.Turno.ToString(),
            EsProductoDescuento = v.EsProductoDescuento,
            DiaSemana = v.Fecha.ToString("dddd", new System.Globalization.CultureInfo("es-ES")),
            SucursalAbreSabadoTarde = v.Sucursal.AbreSabadoTarde
        }).ToList();

        return (ventasDto, totalRegistros);
    }

    public async Task<VentaVendedoraStatsDto> ObtenerEstadisticasAsync(VentaVendedoraFilterDto filtros)
    {
        var query = _context.VentasVendedoras
            .Include(v => v.Sucursal)
            .Include(v => v.Vendedor)
            .AsQueryable();

        query = AplicarFiltros(query, filtros);

        var ventas = await query.ToListAsync();

        if (!ventas.Any())
        {
            return new VentaVendedoraStatsDto();
        }

        var diasConVentas = ventas
            .Where(v => !filtros.ExcluirDomingos || !VentaVendedora.EsDomingo(v.Fecha))
            .GroupBy(v => v.Fecha.Date)
            .Count();

        var topVendedoras = ventas
            .GroupBy(v => v.Vendedor.Nombre)
            .Select(g => new VentaPorVendedoraDto
            {
                VendedorNombre = g.Key,
                TotalVentas = g.Count(),
                MontoTotal = g.Sum(v =>
                    v.Cantidad < 0
                        ? -v.Total  
                        : v.Total),
                CantidadTotal = g.Sum(v => v.EsProductoDescuento && v.Cantidad == -1 ? 0 : v.Cantidad),
                Promedio = diasConVentas > 0 ? g.Sum(v => v.Total) / diasConVentas : 0,
                SucursalesQueTrabaja = g.Select(v => v.Sucursal.Nombre).Distinct().ToList()
            })
            .OrderByDescending(v => v.MontoTotal)
            .Take(5)
            .ToList();

        return new VentaVendedoraStatsDto
        {
            TotalVentas = ventas.Count,
            MontoTotal = ventas.Sum(v =>
                v.Cantidad < 0
                ? -v.Total
                : v.Total),
            CantidadTotal = ventas.Sum(v => v.EsProductoDescuento && v.Cantidad == -1 ? 0 : v.Cantidad),
            PromedioVentaPorDia = diasConVentas > 0 ? ventas.Sum(v => v.Total) / diasConVentas : 0,
            PromedioVentaPorVendedora = ventas.GroupBy(v => v.VendedorId).Any() ? 
                ventas.Sum(v => v.Total) / ventas.GroupBy(v => v.VendedorId).Count() : 0,
            DiasConVentas = diasConVentas,
            TopVendedoras = topVendedoras,
            VentasPorSucursal = await ObtenerVentasPorSucursalAsync(filtros),
            VentasPorTurno = await ObtenerVentasPorTurnoAsync(filtros),
            VentasPorDia = await ObtenerVentasPorDiaAsync(filtros)
        };
    }

    public async Task<List<string>> ObtenerSucursalesAsync()
    {
        return await _context.SucursalesVentas
            .OrderBy(s => s.Nombre)
            .Select(s => s.Nombre)
            .ToListAsync();
    }

    public async Task<List<string>> ObtenerVendedoresAsync()
    {
        return await _context.VendedoresVentas
            .Where(v => v.Activo)
            .OrderBy(v => v.Nombre)
            .Select(v => v.Nombre)
            .ToListAsync();
    }

    public async Task<(DateTime? FechaMinima, DateTime? FechaMaxima)> ObtenerRangoFechasAsync()
    {
        var ventas = await _context.VentasVendedoras
            .OrderBy(v => v.Fecha)
            .Select(v => v.Fecha)
            .ToListAsync();

        if (!ventas.Any())
            return (null, null);

        return (ventas.First(), ventas.Last());
    }

    public async Task<(DateTime? FechaMinima, DateTime? FechaMaxima)> ObtenerUltimaSemanaConDatosAsync()
    {
        var fechaMaxima = await _context.VentasVendedoras
            .MaxAsync(v => (DateTime?)v.Fecha);

        if (!fechaMaxima.HasValue)
            return (null, null);

        var fechaInicio = fechaMaxima.Value.AddDays(-7);
        return (fechaInicio, fechaMaxima.Value);
    }

    public async Task<bool> EliminarVentasPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var ventasAEliminar = await _context.VentasVendedoras
            .Where(v => v.Fecha.Date >= fechaInicio.Date && v.Fecha.Date <= fechaFin.Date)
            .ToListAsync();

        if (!ventasAEliminar.Any())
            return false;

        _context.VentasVendedoras.RemoveRange(ventasAEliminar);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<VentaPorDiaDto>> ObtenerVentasPorDiaAsync(VentaVendedoraFilterDto filtros)
    {
        var query = _context.VentasVendedoras.AsQueryable();
        query = AplicarFiltros(query, filtros);

        var ventasPorDia = await query
            .GroupBy(v => v.Fecha.Date)
            .Select(g => new VentaPorDiaDto
            {
                Fecha = g.Key,
                DiaSemana = g.Key.ToString("dddd"),
                TotalVentas = g.Sum(v => 
                            v.EsProductoDescuento && v.Cantidad == -1 
                                ? 0 
                                : v.Cantidad),
                MontoTotal = g.Sum(v =>
                    v.Cantidad < 0
                        ? -v.Total 
                        : v.Total),
                CantidadTotal = g.Sum(v => v.EsProductoDescuento && v.Cantidad == -1 ? 0 : v.Cantidad),
                EsDomingo = g.Key.DayOfWeek == DayOfWeek.Sunday
            })
            .OrderBy(v => v.Fecha)
            .ToListAsync();

        return ventasPorDia;
    }

    public async Task<List<VentaPorVendedoraDto>> ObtenerTopVendedorasAsync(VentaVendedoraFilterDto filtros, int top = 10)
    {
        var query = _context.VentasVendedoras
            .Include(v => v.Vendedor)
            .Include(v => v.Sucursal)
            .AsQueryable();

        query = AplicarFiltros(query, filtros);

        var diasConVentas = await query
            .Where(v => !filtros.ExcluirDomingos || !VentaVendedora.EsDomingo(v.Fecha))
            .GroupBy(v => v.Fecha.Date)
            .CountAsync();

        var topVendedoras = await query
            .GroupBy(v => v.Vendedor.Nombre)
            .Select(g => new VentaPorVendedoraDto
            {
                VendedorNombre = g.Key,
                TotalVentas = g.Count(),
                MontoTotal = g.Sum(v =>
                    v.Cantidad < 0
                        ? -v.Total 
                        : v.Total),
                CantidadTotal = g.Sum(v => v.EsProductoDescuento && v.Cantidad == -1 ? 0 : v.Cantidad),
                Promedio = diasConVentas > 0 ? g.Sum(v => v.Total) / diasConVentas : 0,
                SucursalesQueTrabaja = g.Select(v => v.Sucursal.Nombre).Distinct().ToList()
            })
            .OrderByDescending(v => v.MontoTotal)
            .Take(top)
            .ToListAsync();

        return topVendedoras;
    }

    public async Task<List<VentaPorSucursalDto>> ObtenerVentasPorSucursalAsync(VentaVendedoraFilterDto filtros)
    {
        var query = _context.VentasVendedoras
            .Include(v => v.Sucursal)
            .AsQueryable();

        query = AplicarFiltros(query, filtros);

        var gruposBase = await query
            .GroupBy(v => new { v.Sucursal.Nombre, v.Sucursal.AbreSabadoTarde })
            .Select(g => new {
                SucursalNombre = g.Key.Nombre,
                TotalVentas = g.Count(),
                MontoTotal = g.Sum(v =>
                    v.Cantidad < 0
                        ? -v.Total 
                        : v.Total),
                AbreSabadoTarde = g.Key.AbreSabadoTarde,
                Ventas = g.ToList()
            })
            .ToListAsync();

        var ventasPorSucursal = gruposBase
            .Select(g => new VentaPorSucursalDto
            {
                SucursalNombre = g.SucursalNombre,
                TotalVentas = g.TotalVentas,
                MontoTotal = g.MontoTotal,
                CantidadTotal = g.Ventas.Sum(v => v.GetCantidadReal()), 
                AbreSabadoTarde = g.AbreSabadoTarde
            })
            .OrderByDescending(v => v.MontoTotal)
            .ToList();

        return ventasPorSucursal;
    }

    public async Task<List<VentaPorTurnoDto>> ObtenerVentasPorTurnoAsync(VentaVendedoraFilterDto filtros)
    {
        var query = _context.VentasVendedoras.AsQueryable();
        query = AplicarFiltros(query, filtros);

        var ventasPorTurno = await query
            .GroupBy(v => v.Turno)
            .Select(g => new VentaPorTurnoDto
            {
                Turno = g.Key.ToString(),
                TotalVentas = g.Sum(v => 
                      v.EsProductoDescuento && v.Cantidad == -1 
                        ? 0 
                        : v.Cantidad),
                MontoTotal = g.Sum(v =>
                    v.Cantidad < 0
                        ? -v.Total  
                        : v.Total),
                CantidadTotal = g.Sum(v => v.EsProductoDescuento && v.Cantidad == -1 ? 0 : v.Cantidad)
            })
            .OrderBy(v => v.Turno)
            .ToListAsync();

        return ventasPorTurno;
    }

    public async Task<bool> ExistenDatosEnRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        return await _context.VentasVendedoras
            .AnyAsync(v => v.Fecha.Date >= fechaInicio.Date && v.Fecha.Date <= fechaFin.Date);
    }

    public async Task<List<string>> ValidarArchivoSinProcesarAsync(Stream archivoStream)
    {
        var errores = new List<string>();

        try
        {
            var resultado = await _excelProcessor.ProcesarArchivoAsync(archivoStream);
            
            if (!resultado.Success)
            {
                errores.Add(resultado.Message);
            }

            errores.AddRange(resultado.Errores);

            // Validaciones adicionales
            if (resultado.Ventas.Any())
            {
                var fechaMin = resultado.FechaMinima;
                var fechaMax = resultado.FechaMaxima;

                if (fechaMin.HasValue && fechaMax.HasValue)
                {
                    var existenDatos = await ExistenDatosEnRangoFechasAsync(fechaMin.Value, fechaMax.Value);
                    if (existenDatos)
                    {
                        errores.Add($"Ya existen datos para el rango {fechaMin:dd/MM/yyyy} - {fechaMax:dd/MM/yyyy}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            errores.Add($"Error al validar archivo: {ex.Message}");
        }

        return errores;
    }

    // Métodos helper privados
    private IQueryable<VentaVendedora> AplicarFiltros(IQueryable<VentaVendedora> query, VentaVendedoraFilterDto filtros)
    {
        if (filtros.FechaInicio.HasValue)
            query = query.Where(v => v.Fecha >= filtros.FechaInicio.Value);

        if (filtros.FechaFin.HasValue)
            query = query.Where(v => v.Fecha <= filtros.FechaFin.Value);

        if (!string.IsNullOrWhiteSpace(filtros.SucursalNombre))
            query = query.Where(v => v.Sucursal.Nombre.Contains(filtros.SucursalNombre));

        if (!string.IsNullOrWhiteSpace(filtros.VendedorNombre))
            query = query.Where(v => v.Vendedor.Nombre.Contains(filtros.VendedorNombre));

        if (!string.IsNullOrWhiteSpace(filtros.Turno))
        {
            if (Enum.TryParse<TurnoVenta>(filtros.Turno, out var turno))
                query = query.Where(v => v.Turno == turno);
        }

        if (filtros.MontoMinimo.HasValue)
            query = query.Where(v => v.Total >= filtros.MontoMinimo.Value);

        if (filtros.MontoMaximo.HasValue)
            query = query.Where(v => v.Total <= filtros.MontoMaximo.Value);

        if (filtros.CantidadMinima.HasValue)
            query = query.Where(v => v.Cantidad >= filtros.CantidadMinima.Value);

        if (filtros.CantidadMaxima.HasValue)
            query = query.Where(v => v.Cantidad <= filtros.CantidadMaxima.Value);

        if (!filtros.IncluirProductosDescuento)
            query = query.Where(v => !v.EsProductoDescuento);

        if (filtros.ExcluirDomingos)
            query = query.Where(v => v.Fecha.DayOfWeek != DayOfWeek.Sunday);

        return query;
    }

    private IQueryable<VentaVendedora> AplicarOrdenamiento(IQueryable<VentaVendedora> query, string orderBy, bool orderDesc)
    {
        return orderBy.ToLower() switch
        {
            "fecha" => orderDesc ? query.OrderByDescending(v => v.Fecha) : query.OrderBy(v => v.Fecha),
            "vendedor" => orderDesc ? query.OrderByDescending(v => v.Vendedor.Nombre) : query.OrderBy(v => v.Vendedor.Nombre),
            "sucursal" => orderDesc ? query.OrderByDescending(v => v.Sucursal.Nombre) : query.OrderBy(v => v.Sucursal.Nombre),
            "total" => orderDesc ? query.OrderByDescending(v => v.Total) : query.OrderBy(v => v.Total),
            "cantidad" => orderDesc ? query.OrderByDescending(v => v.Cantidad) : query.OrderBy(v => v.Cantidad),
            _ => orderDesc ? query.OrderByDescending(v => v.Fecha) : query.OrderBy(v => v.Fecha)
        };
    }

    public async Task<List<VentaPorVendedoraDto>> ObtenerTodasLasVendedorasAsync(VentaVendedoraFilterDto filtros)
    {
        var baseQuery = _context.VentasVendedoras
            .Include(v => v.Vendedor)
            .Include(v => v.Sucursal)
            .AsQueryable();

        var query = AplicarFiltros(baseQuery, filtros);

        if (filtros.ExcluirDomingos)
        {
            query = query.Where(v => v.Fecha.DayOfWeek != DayOfWeek.Sunday);
        }

        var diasConVentas = await query
            .GroupBy(v => v.Fecha.Date)
            .CountAsync();

        var palabrasEspeciales = new[] { "DESCUENTO", "CUPON", "CLUB", "GENERICO", "GIFT", "RESEÑA" };

        var todasVendedoras = await query
            .GroupBy(v => v.Vendedor.Nombre)
            .Select(g => new VentaPorVendedoraDto
            {
                VendedorNombre      = g.Key,
                TotalVentas    = g.Sum(v => 
                         // devoluciones → -1; descuentos/ cupones ignorados; resto positivo
                         v.EsProductoDescuento && v.Cantidad == -1 
                           ? 0 
                           : v.Cantidad),
                MontoTotal = g.Sum(v =>
                    v.Cantidad < 0
                        ? -v.Total 
                        : v.Total),
                CantidadTotal = g.Sum(v =>
                    v.Cantidad < 0
                    ? (palabrasEspeciales.Any(p => 
                            EF.Functions.ILike(v.Producto, "%" + p + "%"))
                        ? 0
                        : v.Cantidad)
                    : v.Cantidad),
                Promedio            = g.Count() > 0
                                        ? g.Sum(v => v.Total) / (decimal)g.Count()
                                        : 0m,
                SucursalesQueTrabaja = g.Select(v => v.Sucursal.Nombre)
                                        .Distinct()
                                        .ToList()
            })
            .OrderByDescending(v => v.MontoTotal)
            .ToListAsync();

        return todasVendedoras;
    }
}