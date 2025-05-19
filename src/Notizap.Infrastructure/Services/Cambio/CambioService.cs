using AutoMapper;
using Microsoft.EntityFrameworkCore;

public class CambioService : ICambioService
{
    private readonly NotizapDbContext _context;
    private readonly IMapper _mapper;

    public CambioService(NotizapDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<int> CrearCambioAsync(CreateCambioDto dto)
    {
        var cambio = _mapper.Map<Cambio>(dto);
        cambio.LlegoAlDeposito = false;
        cambio.YaEnviado = false;
        cambio.CambioRegistradoSistema = false;

        cambio.Fecha = DateTime.SpecifyKind(cambio.Fecha, DateTimeKind.Utc);
        cambio.FechaRetiro = DateTime.SpecifyKind(cambio.FechaRetiro, DateTimeKind.Utc);

        _context.Cambios.Add(cambio);
        await _context.SaveChangesAsync();
        return cambio.Id;
    }

    public async Task<List<CambioDto>> ObtenerTodosAsync()
    {
        var cambios = await _context.Cambios
            .OrderByDescending(c => c.Fecha)
            .ToListAsync();

        return _mapper.Map<List<CambioDto>>(cambios);
    }

    public async Task<CambioDto?> ObtenerPorIdAsync(int id)
    {
        var cambio = await _context.Cambios.FindAsync(id);
        return cambio == null ? null : _mapper.Map<CambioDto>(cambio);
    }

    public async Task<bool> ActualizarCambioAsync(int id, CambioDto dto)
    {
        var existente = await _context.Cambios.FindAsync(id);
        if (existente == null) return false;

        _mapper.Map(dto, existente); // sobrescribe los valores
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarCambioAsync(int id)
    {
        var cambio = await _context.Cambios.FindAsync(id);
        if (cambio == null) return false;

        _context.Cambios.Remove(cambio);
        await _context.SaveChangesAsync();
        return true;
    }
}
