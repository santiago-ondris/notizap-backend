using Microsoft.EntityFrameworkCore;

public class VentaWooCommerceService : IVentaWooCommerceService
{
    private readonly NotizapDbContext _context;

    public VentaWooCommerceService(NotizapDbContext context)
    {
        _context = context;
    }

    // CRUD Básico
    public async Task<VentaWooCommerceDto> GetByIdAsync(int id)
    {
        var venta = await _context.VentasWooCommerce
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta == null)
            throw new KeyNotFoundException($"Venta con ID {id} no encontrada");

        return MapToDto(venta);
    }

    public async Task<IEnumerable<VentaWooCommerceDto>> GetAllAsync()
    {
        var ventas = await _context.VentasWooCommerce
            .OrderByDescending(v => v.FechaCreacion)
            .ToListAsync();

        return ventas.Select(MapToDto);
    }

    public async Task<VentaWooCommerceDto> CreateAsync(CreateVentaWooCommerceDto createDto)
    {
        // Validar período
        if (!VentaWooCommerceHelpers.EsPeriodoValido(createDto.Mes, createDto.Año))
            throw new ArgumentException("Período inválido");

        // Validar duplicado
        var existeRegistro = await ExistsAsync(createDto.Tienda, createDto.Mes, createDto.Año);
        if (existeRegistro)
            throw new InvalidOperationException($"Ya existe un registro para {createDto.Tienda} en {VentaWooCommerceHelpers.FormatearPeriodo(createDto.Mes, createDto.Año)}");

        var venta = new VentaWooCommerce
        {
            Tienda = VentaWooCommerceHelpers.NormalizarTienda(createDto.Tienda),
            Mes = createDto.Mes,
            Año = createDto.Año,
            MontoFacturado = createDto.MontoFacturado,
            UnidadesVendidas = createDto.UnidadesVendidas,
            TopProductos = VentaWooCommerceHelpers.SerializeList(VentaWooCommerceHelpers.LimpiarYValidarLista(createDto.TopProductos)),
            TopCategorias = VentaWooCommerceHelpers.SerializeList(VentaWooCommerceHelpers.LimpiarYValidarLista(createDto.TopCategorias)),
            FechaCreacion = DateTime.UtcNow
        };

        _context.VentasWooCommerce.Add(venta);
        await _context.SaveChangesAsync();

        return MapToDto(venta);
    }

    public async Task<VentaWooCommerceDto> UpdateAsync(UpdateVentaWooCommerceDto updateDto)
    {
        var venta = await _context.VentasWooCommerce
            .FirstOrDefaultAsync(v => v.Id == updateDto.Id);

        if (venta == null)
            throw new KeyNotFoundException($"Venta con ID {updateDto.Id} no encontrada");

        // Validar período
        if (!VentaWooCommerceHelpers.EsPeriodoValido(updateDto.Mes, updateDto.Año))
            throw new ArgumentException("Período inválido");

        // Validar duplicado (excluyendo el registro actual)
        var existeOtroRegistro = await _context.VentasWooCommerce
            .AnyAsync(v => v.Id != updateDto.Id &&
                            VentaWooCommerceHelpers.NormalizarTienda(v.Tienda) == VentaWooCommerceHelpers.NormalizarTienda(updateDto.Tienda) &&
                            v.Mes == updateDto.Mes &&
                            v.Año == updateDto.Año);

        if (existeOtroRegistro)
            throw new InvalidOperationException($"Ya existe otro registro para {updateDto.Tienda} en {VentaWooCommerceHelpers.FormatearPeriodo(updateDto.Mes, updateDto.Año)}");

        venta.Tienda = VentaWooCommerceHelpers.NormalizarTienda(updateDto.Tienda);
        venta.Mes = updateDto.Mes;
        venta.Año = updateDto.Año;
        venta.MontoFacturado = updateDto.MontoFacturado;
        venta.UnidadesVendidas = updateDto.UnidadesVendidas;
        venta.TopProductos = VentaWooCommerceHelpers.SerializeList(VentaWooCommerceHelpers.LimpiarYValidarLista(updateDto.TopProductos));
        venta.TopCategorias = VentaWooCommerceHelpers.SerializeList(VentaWooCommerceHelpers.LimpiarYValidarLista(updateDto.TopCategorias));
        venta.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(venta);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var venta = await _context.VentasWooCommerce
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta == null)
            return false;

        _context.VentasWooCommerce.Remove(venta);
        await _context.SaveChangesAsync();

        return true;
    }

    // Consultas con filtros y paginación
    public async Task<(IEnumerable<VentaWooCommerceDto> Items, int TotalCount)> GetPagedAsync(VentaWooCommerceQueryDto queryDto)
    {
        var query = VentaWooCommerceHelpers.ValidarYLimpiarQuery(queryDto);
        
        var ventasQuery = _context.VentasWooCommerce.AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(query.Tienda))
            ventasQuery = ventasQuery.Where(v => v.Tienda.Contains(query.Tienda));

        if (query.Mes.HasValue)
            ventasQuery = ventasQuery.Where(v => v.Mes == query.Mes.Value);

        if (query.Año.HasValue)
            ventasQuery = ventasQuery.Where(v => v.Año == query.Año.Value);

        if (query.FechaDesde.HasValue)
            ventasQuery = ventasQuery.Where(v => v.FechaCreacion >= query.FechaDesde.Value);

        if (query.FechaHasta.HasValue)
            ventasQuery = ventasQuery.Where(v => v.FechaCreacion <= query.FechaHasta.Value);

        // Aplicar ordenamiento
        ventasQuery = query.OrderBy switch
        {
            "Mes" => query.OrderDescending ? ventasQuery.OrderByDescending(v => v.Mes) : ventasQuery.OrderBy(v => v.Mes),
            "Año" => query.OrderDescending ? ventasQuery.OrderByDescending(v => v.Año) : ventasQuery.OrderBy(v => v.Año),
            "MontoFacturado" => query.OrderDescending ? ventasQuery.OrderByDescending(v => v.MontoFacturado) : ventasQuery.OrderBy(v => v.MontoFacturado),
            "UnidadesVendidas" => query.OrderDescending ? ventasQuery.OrderByDescending(v => v.UnidadesVendidas) : ventasQuery.OrderBy(v => v.UnidadesVendidas),
            "Tienda" => query.OrderDescending ? ventasQuery.OrderByDescending(v => v.Tienda) : ventasQuery.OrderBy(v => v.Tienda),
            _ => query.OrderDescending ? ventasQuery.OrderByDescending(v => v.FechaCreacion) : ventasQuery.OrderBy(v => v.FechaCreacion)
        };

        var totalCount = await ventasQuery.CountAsync();

        var ventas = await ventasQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (ventas.Select(MapToDto), totalCount);
    }

    // Consultas específicas para el negocio
    public async Task<VentaWooCommerceDto?> GetByTiendaYPeriodoAsync(string tienda, int mes, int año)
    {
        var tiendaNormalizada = VentaWooCommerceHelpers.NormalizarTienda(tienda);
        
        var venta = await _context.VentasWooCommerce
            .FirstOrDefaultAsync(v => v.Tienda == tiendaNormalizada && v.Mes == mes && v.Año == año);

        return venta != null ? MapToDto(venta) : null;
    }

    public async Task<IEnumerable<VentaWooCommerceDto>> GetByPeriodoAsync(int mes, int año)
    {
        var ventas = await _context.VentasWooCommerce
            .Where(v => v.Mes == mes && v.Año == año)
            .OrderBy(v => v.Tienda)
            .ToListAsync();

        return ventas.Select(MapToDto);
    }

    public async Task<IEnumerable<VentaWooCommerceDto>> GetByTiendaAsync(string tienda)
    {
        var tiendaNormalizada = VentaWooCommerceHelpers.NormalizarTienda(tienda);
        
        var ventas = await _context.VentasWooCommerce
            .Where(v => v.Tienda == tiendaNormalizada)
            .OrderByDescending(v => v.Año)
            .ThenByDescending(v => v.Mes)
            .ToListAsync();

        return ventas.Select(MapToDto);
    }

    public async Task<IEnumerable<VentaWooCommerceDto>> GetByAñoAsync(int año)
    {
        var ventas = await _context.VentasWooCommerce
            .Where(v => v.Año == año)
            .OrderBy(v => v.Mes)
            .ThenBy(v => v.Tienda)
            .ToListAsync();

        return ventas.Select(MapToDto);
    }

    // Dashboard y reportes (como tu Excel)
    public async Task<TotalesVentasDto> GetTotalesByPeriodoAsync(int mes, int año)
    {
        var ventas = await GetByPeriodoAsync(mes, año);
        return VentaWooCommerceHelpers.GenerarTotales(ventas, mes, año);
    }

    public async Task<IEnumerable<ResumenVentasDto>> GetResumenByPeriodoAsync(int mes, int año)
    {
        var totales = await GetTotalesByPeriodoAsync(mes, año);
        return totales.VentasPorTienda;
    }

    public async Task<IEnumerable<TotalesVentasDto>> GetTotalesByAñoAsync(int año)
    {
        var totalesPorMes = new List<TotalesVentasDto>();

        for (int mes = 1; mes <= 12; mes++)
        {
            var totales = await GetTotalesByPeriodoAsync(mes, año);
            totalesPorMes.Add(totales);
        }

        return totalesPorMes;
    }

    // Validaciones de negocio
    public async Task<bool> ExistsAsync(string tienda, int mes, int año)
    {
        var tiendaNormalizada = VentaWooCommerceHelpers.NormalizarTienda(tienda);
        
        return await _context.VentasWooCommerce
            .AnyAsync(v => v.Tienda == tiendaNormalizada && v.Mes == mes && v.Año == año);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.VentasWooCommerce
            .AnyAsync(v => v.Id == id);
    }

    // Estadísticas adicionales
    public async Task<decimal> GetTotalFacturadoByTiendaAsync(string tienda, int año)
    {
        var tiendaNormalizada = VentaWooCommerceHelpers.NormalizarTienda(tienda);
        
        return await _context.VentasWooCommerce
            .Where(v => v.Tienda == tiendaNormalizada && v.Año == año)
            .SumAsync(v => v.MontoFacturado);
    }

    public async Task<int> GetTotalUnidadesByTiendaAsync(string tienda, int año)
    {
        var tiendaNormalizada = VentaWooCommerceHelpers.NormalizarTienda(tienda);
        
        return await _context.VentasWooCommerce
            .Where(v => v.Tienda == tiendaNormalizada && v.Año == año)
            .SumAsync(v => v.UnidadesVendidas);
    }

    public async Task<IEnumerable<string>> GetTiendasDisponiblesAsync()
    {
        return await _context.VentasWooCommerce
            .Select(v => v.Tienda)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();
    }

    public async Task<IEnumerable<int>> GetAñosDisponiblesAsync()
    {
        return await _context.VentasWooCommerce
            .Select(v => v.Año)
            .Distinct()
            .OrderByDescending(a => a)
            .ToListAsync();
    }

    // Operaciones de lote
    public async Task<IEnumerable<VentaWooCommerceDto>> CreateBatchAsync(IEnumerable<CreateVentaWooCommerceDto> createDtos)
    {
        var ventasCreadas = new List<VentaWooCommerce>();

        foreach (var createDto in createDtos)
        {
            // Validar período
            if (!VentaWooCommerceHelpers.EsPeriodoValido(createDto.Mes, createDto.Año))
                throw new ArgumentException($"Período inválido para {createDto.Tienda}: {VentaWooCommerceHelpers.FormatearPeriodo(createDto.Mes, createDto.Año)}");

            // Validar duplicado
            var existeRegistro = await ExistsAsync(createDto.Tienda, createDto.Mes, createDto.Año);
            if (existeRegistro)
                throw new InvalidOperationException($"Ya existe un registro para {createDto.Tienda} en {VentaWooCommerceHelpers.FormatearPeriodo(createDto.Mes, createDto.Año)}");

            var venta = new VentaWooCommerce
            {
                Tienda = VentaWooCommerceHelpers.NormalizarTienda(createDto.Tienda),
                Mes = createDto.Mes,
                Año = createDto.Año,
                MontoFacturado = createDto.MontoFacturado,
                UnidadesVendidas = createDto.UnidadesVendidas,
                TopProductos = VentaWooCommerceHelpers.SerializeList(VentaWooCommerceHelpers.LimpiarYValidarLista(createDto.TopProductos)),
                TopCategorias = VentaWooCommerceHelpers.SerializeList(VentaWooCommerceHelpers.LimpiarYValidarLista(createDto.TopCategorias)),
                FechaCreacion = DateTime.UtcNow
            };

            ventasCreadas.Add(venta);
        }

        _context.VentasWooCommerce.AddRange(ventasCreadas);
        await _context.SaveChangesAsync();

        return ventasCreadas.Select(MapToDto);
    }

    public async Task<bool> DeleteByPeriodoAsync(int mes, int año)
    {
        var ventas = await _context.VentasWooCommerce
            .Where(v => v.Mes == mes && v.Año == año)
            .ToListAsync();

        if (!ventas.Any())
            return false;

        _context.VentasWooCommerce.RemoveRange(ventas);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteByTiendaAsync(string tienda)
    {
        var tiendaNormalizada = VentaWooCommerceHelpers.NormalizarTienda(tienda);
        
        var ventas = await _context.VentasWooCommerce
            .Where(v => v.Tienda == tiendaNormalizada)
            .ToListAsync();

        if (!ventas.Any())
            return false;

        _context.VentasWooCommerce.RemoveRange(ventas);
        await _context.SaveChangesAsync();

        return true;
    }

    // Métodos de comparación temporal
    public async Task<decimal> GetCrecimientoMensualAsync(string tienda, int mesActual, int añoActual, int mesAnterior, int añoAnterior)
    {
        var ventaActual = await GetByTiendaYPeriodoAsync(tienda, mesActual, añoActual);
        var ventaAnterior = await GetByTiendaYPeriodoAsync(tienda, mesAnterior, añoAnterior);

        var montoActual = ventaActual?.MontoFacturado ?? 0;
        var montoAnterior = ventaAnterior?.MontoFacturado ?? 0;

        return VentaWooCommerceHelpers.CalcularCrecimientoPorcentual(montoActual, montoAnterior);
    }

    public async Task<IEnumerable<TotalesVentasDto>> GetEvolucionAnualAsync(string tienda, int año)
    {
        var evolucion = new List<TotalesVentasDto>();

        for (int mes = 1; mes <= 12; mes++)
        {
            var venta = await GetByTiendaYPeriodoAsync(tienda, mes, año);
            
            var total = new TotalesVentasDto
            {
                TotalFacturado = venta?.MontoFacturado ?? 0,
                TotalUnidades = venta?.UnidadesVendidas ?? 0,
                VentasPorTienda = venta != null ? new List<ResumenVentasDto> 
                { 
                    new ResumenVentasDto
                    {
                        Tienda = venta.Tienda,
                        MontoFacturado = venta.MontoFacturado,
                        UnidadesVendidas = venta.UnidadesVendidas,
                        TopProductos = venta.TopProductos,
                        TopCategorias = venta.TopCategorias
                    }
                } : new List<ResumenVentasDto>(),
                Mes = mes,
                Año = año,
                PeriodoCompleto = VentaWooCommerceHelpers.FormatearPeriodo(mes, año)
            };

            evolucion.Add(total);
        }

        return evolucion;
    }

    // Mapping
    public VentaWooCommerceDto MapToDto(VentaWooCommerce venta)
    {
        return new VentaWooCommerceDto
        {
            Id = venta.Id,
            Tienda = venta.Tienda,
            Mes = venta.Mes,
            Año = venta.Año,
            MontoFacturado = venta.MontoFacturado,
            UnidadesVendidas = venta.UnidadesVendidas,
            TopProductos = VentaWooCommerceHelpers.DeserializeList(venta.TopProductos),
            TopCategorias = VentaWooCommerceHelpers.DeserializeList(venta.TopCategorias),
            FechaCreacion = venta.FechaCreacion,
            FechaActualizacion = venta.FechaActualizacion,
            PeriodoCompleto = VentaWooCommerceHelpers.FormatearPeriodo(venta.Mes, venta.Año)
        };
    }
}