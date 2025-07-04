using Microsoft.EntityFrameworkCore;
public class ComisionOnlineService : IComisionOnlineService
{
    private readonly NotizapDbContext _context;

    public ComisionOnlineService(NotizapDbContext context)
    {
        _context = context;
    }

    // CRUD Básico
    public async Task<ComisionOnlineDto> GetByIdAsync(int id)
    {
        var comision = await _context.ComisionesOnline
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comision == null)
            throw new KeyNotFoundException($"Comisión con ID {id} no encontrada");

        return MapToDto(comision);
    }

    public async Task<IEnumerable<ComisionOnlineDto>> GetAllAsync()
    {
        var comisiones = await _context.ComisionesOnline
            .OrderByDescending(c => c.Año)
            .ThenByDescending(c => c.Mes)
            .ToListAsync();

        return comisiones.Select(MapToDto);
    }

    public async Task<ComisionOnlineDto> CreateAsync(CreateComisionOnlineDto createDto)
    {
        // Validar período
        if (!EsPeriodoValido(createDto.Mes, createDto.Año))
            throw new ArgumentException("Período inválido");

        // Validar duplicado
        var existeRegistro = await ExistsAsync(createDto.Mes, createDto.Año);
        if (existeRegistro)
            throw new InvalidOperationException($"Ya existe un registro de comisiones para {FormatearPeriodo(createDto.Mes, createDto.Año)}");

        var comision = new ComisionOnline
        {
            Mes = createDto.Mes,
            Año = createDto.Año,
            TotalSinNC = createDto.TotalSinNC,
            MontoAndreani = createDto.MontoAndreani,
            MontoOCA = createDto.MontoOCA,
            MontoCaddy = createDto.MontoCaddy,
            FechaCreacion = DateTime.UtcNow
        };

        _context.ComisionesOnline.Add(comision);
        await _context.SaveChangesAsync();

        return MapToDto(comision);
    }

    public async Task<ComisionOnlineDto> UpdateAsync(UpdateComisionOnlineDto updateDto)
    {
        var comision = await _context.ComisionesOnline
            .FirstOrDefaultAsync(c => c.Id == updateDto.Id);

        if (comision == null)
            throw new KeyNotFoundException($"Comisión con ID {updateDto.Id} no encontrada");

        // Validar período
        if (!EsPeriodoValido(updateDto.Mes, updateDto.Año))
            throw new ArgumentException("Período inválido");

        // Validar duplicado (excluyendo el registro actual)
        var existeOtroRegistro = await _context.ComisionesOnline
            .AnyAsync(c => c.Id != updateDto.Id &&
                            c.Mes == updateDto.Mes &&
                            c.Año == updateDto.Año);

        if (existeOtroRegistro)
            throw new InvalidOperationException($"Ya existe otro registro de comisiones para {FormatearPeriodo(updateDto.Mes, updateDto.Año)}");

        comision.Mes = updateDto.Mes;
        comision.Año = updateDto.Año;
        comision.TotalSinNC = updateDto.TotalSinNC;
        comision.MontoAndreani = updateDto.MontoAndreani;
        comision.MontoOCA = updateDto.MontoOCA;
        comision.MontoCaddy = updateDto.MontoCaddy;
        comision.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(comision);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var comision = await _context.ComisionesOnline
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comision == null)
            return false;

        _context.ComisionesOnline.Remove(comision);
        await _context.SaveChangesAsync();

        return true;
    }

    // Consultas con filtros y paginación
    public async Task<(IEnumerable<ComisionOnlineDto> Items, int TotalCount)> GetPagedAsync(ComisionOnlineQueryDto queryDto)
    {
        var query = ValidarYLimpiarQuery(queryDto);
        var comisionesQuery = _context.ComisionesOnline.AsQueryable();

        // Aplicar filtros
        if (query.Mes.HasValue)
            comisionesQuery = comisionesQuery.Where(c => c.Mes == query.Mes.Value);

        if (query.Año.HasValue)
            comisionesQuery = comisionesQuery.Where(c => c.Año == query.Año.Value);

        if (query.FechaDesde.HasValue)
            comisionesQuery = comisionesQuery.Where(c => c.FechaCreacion >= query.FechaDesde.Value);

        if (query.FechaHasta.HasValue)
            comisionesQuery = comisionesQuery.Where(c => c.FechaCreacion <= query.FechaHasta.Value);

        // Aplicar ordenamiento
        comisionesQuery = query.OrderBy switch
        {
            "Mes" => query.OrderDescending ? comisionesQuery.OrderByDescending(c => c.Mes) : comisionesQuery.OrderBy(c => c.Mes),
            "Año" => query.OrderDescending ? comisionesQuery.OrderByDescending(c => c.Año) : comisionesQuery.OrderBy(c => c.Año),
            "TotalSinNC" => query.OrderDescending ? comisionesQuery.OrderByDescending(c => c.TotalSinNC) : comisionesQuery.OrderBy(c => c.TotalSinNC),
            _ => query.OrderDescending ? comisionesQuery.OrderByDescending(c => c.FechaCreacion) : comisionesQuery.OrderBy(c => c.FechaCreacion)
        };

        var totalCount = await comisionesQuery.CountAsync();

        var comisiones = await comisionesQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (comisiones.Select(MapToDto), totalCount);
    }

    // Consultas específicas del negocio
    public async Task<ComisionOnlineDto?> GetByPeriodoAsync(int mes, int año)
    {
        var comision = await _context.ComisionesOnline
            .FirstOrDefaultAsync(c => c.Mes == mes && c.Año == año);

        return comision != null ? MapToDto(comision) : null;
    }

    public async Task<IEnumerable<ComisionOnlineDto>> GetByAñoAsync(int año)
    {
        var comisiones = await _context.ComisionesOnline
            .Where(c => c.Año == año)
            .OrderBy(c => c.Mes)
            .ToListAsync();

        return comisiones.Select(MapToDto);
    }

    // Validaciones
    public async Task<bool> ExistsAsync(int mes, int año)
    {
        return await _context.ComisionesOnline
            .AnyAsync(c => c.Mes == mes && c.Año == año);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.ComisionesOnline
            .AnyAsync(c => c.Id == id);
    }

  // Cálculos
  public CalculoComisionDto CalcularComision(int mes, int año, decimal totalSinNC, decimal montoAndreani, decimal montoOCA, decimal montoCaddy)
  {
    var totalEnvios = montoAndreani + montoOCA + montoCaddy;
    var baseCalculo = totalSinNC - totalEnvios;
    var baseCalculoSinIVA = baseCalculo * 0.79m; // Se resta el 21%
    var comisionBruta = baseCalculoSinIVA * 0.005m;
    var comisionPorPersona = comisionBruta / 6m;

    return new CalculoComisionDto
    {
      TotalSinNC = totalSinNC,
      TotalEnvios = totalEnvios,
      BaseCalculo = baseCalculo,
      BaseCalculoSinIVA = baseCalculoSinIVA,
      ComisionBruta = comisionBruta,
      ComisionPorPersona = comisionPorPersona,
      DetalleEnvios = new List<DetalleEnvioDto>
            {
                new() { Empresa = "ANDREANI", Monto = montoAndreani },
                new() { Empresa = "OCA", Monto = montoOCA },
                new() { Empresa = "CADDY", Monto = montoCaddy }
            }
    };
  }

  // Operaciones de lote
  public async Task<bool> DeleteByPeriodoAsync(int mes, int año)
    {
        var comision = await _context.ComisionesOnline
            .FirstOrDefaultAsync(c => c.Mes == mes && c.Año == año);

        if (comision == null)
            return false;

        _context.ComisionesOnline.Remove(comision);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<int>> GetAñosDisponiblesAsync()
    {
        return await _context.ComisionesOnline
            .Select(c => c.Año)
            .Distinct()
            .OrderByDescending(a => a)
            .ToListAsync();
    }

    // Métodos de mapeo y helpers
    private ComisionOnlineDto MapToDto(ComisionOnline comision)
    {
        return new ComisionOnlineDto
        {
            Id = comision.Id,
            Mes = comision.Mes,
            Año = comision.Año,
            TotalSinNC = comision.TotalSinNC,
            MontoAndreani = comision.MontoAndreani,
            MontoOCA = comision.MontoOCA,
            MontoCaddy = comision.MontoCaddy,
            FechaCreacion = comision.FechaCreacion,
            FechaActualizacion = comision.FechaActualizacion,
            TotalEnvios = comision.TotalEnvios,
            BaseCalculo = comision.BaseCalculo,
            BaseCalculoSinIVA = comision.BaseCalculoSinIVA,
            ComisionBruta = comision.ComisionBruta,
            ComisionPorPersona = comision.ComisionPorPersona,
            PeriodoCompleto = comision.PeriodoCompleto
        };
    }

    private static bool EsPeriodoValido(int mes, int año)
    {
        return mes >= 1 && mes <= 12 && año >= 2020 && año <= 2030;
    }

    private static string FormatearPeriodo(int mes, int año)
    {
        var meses = new[] { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
                            "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
        return $"{meses[mes - 1]} {año}";
    }

    private static ComisionOnlineQueryDto ValidarYLimpiarQuery(ComisionOnlineQueryDto query)
    {
        return new ComisionOnlineQueryDto
        {
            Mes = query.Mes,
            Año = query.Año,
            FechaDesde = query.FechaDesde,
            FechaHasta = query.FechaHasta,
            PageNumber = Math.Max(1, query.PageNumber),
            PageSize = Math.Min(100, Math.Max(1, query.PageSize)),
            OrderBy = string.IsNullOrWhiteSpace(query.OrderBy) ? "FechaCreacion" : query.OrderBy,
            OrderDescending = query.OrderDescending
        };
    }
}