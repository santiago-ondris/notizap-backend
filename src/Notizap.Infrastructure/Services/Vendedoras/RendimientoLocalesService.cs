using Microsoft.EntityFrameworkCore;

public class RendimientoLocalesService : IRendimientoLocalesService
{
    private readonly NotizapDbContext _context;

    public RendimientoLocalesService(NotizapDbContext context)
    {
        _context = context;
    }

    public async Task<RendimientoLocalesResponseDto> ObtenerRendimientoLocalesAsync(RendimientoLocalesFilterDto filtros)
    {
        var fechaInicio = filtros.FechaInicio.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(filtros.FechaInicio, DateTimeKind.Utc)
            : filtros.FechaInicio.ToUniversalTime();
        var fechaFin = filtros.FechaFin.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(filtros.FechaFin, DateTimeKind.Utc)
            : filtros.FechaFin.ToUniversalTime();

        var filtrosVentas = new VentaVendedoraFilterDto
        {
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            SucursalNombre = filtros.SucursalNombre,
            Turno = filtros.Turno,
            ExcluirDomingos = filtros.ExcluirDomingos,
        };

        var ventasQuery = _context.VentasVendedoras
            .Include(v => v.Vendedor)
            .Include(v => v.Sucursal)
            .AsNoTracking();

        ventasQuery = AplicarFiltros(ventasQuery, filtrosVentas);

        var ventas = await ventasQuery.ToListAsync();

        // Mapear a DTO para el helper (usá tu propio mapper o manual)
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

        // Calcular agrupaciones y comparaciones
        var dias = RendimientoLocalesHelper.CalcularRendimientoPorDia(
            ventasDto,
            filtros.SucursalNombre,
            filtros.Turno
        );

        // Calcular resumen por vendedora (la métrica a comparar viene en filtros)
        var resumenVendedoras = RendimientoLocalesHelper.CalcularResumenPorVendedora(
            dias, filtros.MetricaComparar ?? "monto"
        );

        // Paginación de días
        var totalDias = dias.Count;
        var page = filtros.Page > 0 ? filtros.Page : 1;
        var pageSize = filtros.PageSize > 0 ? filtros.PageSize : 31;
        var diasPaginados = dias.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var totalPaginas = (int)Math.Ceiling((double)totalDias / pageSize);

        return new RendimientoLocalesResponseDto
        {
            Dias = diasPaginados,
            ResumenVendedoras = resumenVendedoras,
            TotalDias = totalDias,
            Pagina = page,
            PageSize = pageSize,
            TotalPaginas = totalPaginas
        };
        
    }

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
}
