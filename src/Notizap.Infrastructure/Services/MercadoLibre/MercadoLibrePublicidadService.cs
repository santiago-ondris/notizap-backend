using AutoMapper;
using Microsoft.EntityFrameworkCore;

public class MercadoLibrePublicidadService : IMercadoLibrePublicidadService
{
    private readonly NotizapDbContext _context;
    private readonly IMapper _mapper;

    public MercadoLibrePublicidadService(NotizapDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<int> CrearProductAdAsync(CreateProductAdDto dto)
    {
        var entidad = _mapper.Map<ReportePublicidadML>(dto);
        await _context.ReportesPublicidadML.AddAsync(entidad);
        await _context.SaveChangesAsync();
        return entidad.Id;
    }

    public async Task<int> CrearBrandAdAsync(CreateBrandAdDto dto)
    {
        var entidad = _mapper.Map<ReportePublicidadML>(dto);
        await _context.ReportesPublicidadML.AddAsync(entidad);
        await _context.SaveChangesAsync();
        return entidad.Id;
    }

    public async Task<int> CrearDisplayAdAsync(CreateDisplayAdDto dto)
    {
        var entidad = _mapper.Map<ReportePublicidadML>(dto);
        entidad.Anuncios = _mapper.Map<List<AnuncioDisplayML>>(dto.Anuncios);
        await _context.ReportesPublicidadML.AddAsync(entidad);
        await _context.SaveChangesAsync();
        return entidad.Id;
    }

    public async Task<List<ReadAdDto>> ObtenerTodosAsync()
    {
        var entidades = await _context.ReportesPublicidadML
            .Include(r => r.Anuncios)
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Month)
            .ToListAsync();

        return entidades.Select(MapToReadDto).ToList();
    }

    public async Task<List<ReadAdDto>> ObtenerPorMesAsync(int year, int month)
    {
        var entidades = await _context.ReportesPublicidadML
            .Where(r => r.Year == year && r.Month == month)
            .Include(r => r.Anuncios)
            .ToListAsync();

        return entidades.Select(MapToReadDto).ToList();
    }

    public async Task<ReadAdDto?> ObtenerPorIdAsync(int id)
    {
        var entidad = await _context.ReportesPublicidadML
            .Include(r => r.Anuncios)
            .FirstOrDefaultAsync(r => r.Id == id);

        return entidad == null ? null : MapToReadDto(entidad);
    }

    public async Task<decimal> ObtenerInversionTotalPorMesAsync(int year, int month)
    {
        return await _context.ReportesPublicidadML
            .Where(r => r.Year == year && r.Month == month)
            .SumAsync(r => r.Inversion);
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var entidad = await _context.ReportesPublicidadML.FindAsync(id);
        if (entidad == null) return false;

        _context.ReportesPublicidadML.Remove(entidad);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ReportePublicidadML>> ObtenerPorTipoYMesAsync(TipoPublicidadML tipo, int year, int month)
    {
        return await _context.ReportesPublicidadML
            .Where(r => r.Tipo == tipo && r.Year == year && r.Month == month)
            .Include(r => r.Anuncios)
            .ToListAsync();
    }

    private ReadAdDto MapToReadDto(ReportePublicidadML entidad)
    {
        var dto = new ReadAdDto
        {
            Id = entidad.Id,
            NombreCampania = entidad.NombreCampania,
            Tipo = entidad.Tipo.ToString(),
            Inversion = entidad.Inversion,
            Year = entidad.Year,
            Month = entidad.Month
        };

        switch (entidad.Tipo)
        {
            case TipoPublicidadML.ProductAds:
                dto.Detalles = new ProductAdDetailsDto
                {
                    AcosObjetivo = entidad.AcosObjetivo ?? 0,
                    VentasPads = entidad.VentasPads ?? 0,
                    AcosReal = entidad.AcosReal ?? 0,
                    Impresiones = entidad.Impresiones ?? 0,
                    Clics = entidad.Clics ?? 0,
                    Ingresos = entidad.Ingresos ?? 0
                };
                break;

            case TipoPublicidadML.BrandAds:
                dto.Detalles = new BrandAdDetailsDto
                {
                    Presupuesto = entidad.Presupuesto ?? 0,
                    Ventas = entidad.Ventas ?? 0,
                    Clics = entidad.Clics ?? 0,
                    Ingresos = entidad.Ingresos ?? 0,
                    Cpc = entidad.Cpc ?? 0
                };
                break;

            case TipoPublicidadML.DisplayAds:
                dto.Detalles = new DisplayAdDetailsDto
                {
                    VisitasConDisplay = entidad.VisitasConDisplay ?? 0,
                    VisitasSinDisplay = entidad.VisitasSinDisplay ?? 0,
                    Clics = entidad.Clics ?? 0,
                    Impresiones = entidad.Impresiones ?? 0,
                    Alcance = entidad.Alcance ?? 0,
                    CostoPorVisita = entidad.CostoPorVisita ?? 0,
                    Anuncios = entidad.Anuncios.Select(a => new DisplayAnuncioDto
                    {
                        Nombre = a.Nombre,
                        Impresiones = a.Impresiones,
                        Clics = a.Clics,
                        Visitas = a.Visitas,
                        Ctr = a.Ctr
                    }).ToList()
                };
                break;
        }

        return dto;
    }
}
