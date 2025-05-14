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
}
