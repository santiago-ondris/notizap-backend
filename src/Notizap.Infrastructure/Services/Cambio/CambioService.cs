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

    public async Task<int> CrearCambioAsync(CreateCambioSimpleDto dto)
    {
        var cambio = _mapper.Map<Cambio>(dto);
        
        // Setear estados iniciales
        cambio.LlegoAlDeposito = false;
        cambio.YaEnviado = false;
        cambio.CambioRegistradoSistema = false;

        // Configurar fecha como UTC
        cambio.Fecha = DateTime.SpecifyKind(cambio.Fecha, DateTimeKind.Utc);

        _context.Cambios.Add(cambio);
        await _context.SaveChangesAsync();
        return cambio.Id;
    }

    public async Task<List<CambioSimpleDto>> ObtenerTodosAsync()
    {
        var cambios = await _context.Cambios
            .OrderByDescending(c => c.Fecha)
            .ToListAsync();

        return _mapper.Map<List<CambioSimpleDto>>(cambios);
    }

    public async Task<CambioSimpleDto?> ObtenerPorIdAsync(int id)
    {
        var cambio = await _context.Cambios.FindAsync(id);
        return cambio == null ? null : _mapper.Map<CambioSimpleDto>(cambio);
    }

    public async Task<bool> ActualizarCambioAsync(int id, CambioSimpleDto dto)
    {
        var existente = await _context.Cambios.FindAsync(id);
        if (existente == null) return false;

        // Mapear campos del DTO
        existente.Fecha = DateTime.SpecifyKind(dto.Fecha, DateTimeKind.Utc);
        existente.Pedido = dto.Pedido;
        existente.Celular = dto.Celular;
        existente.Nombre = dto.Nombre;
        existente.Apellido = dto.Apellido ?? string.Empty;
        existente.ModeloOriginal = dto.ModeloOriginal;
        existente.ModeloCambio = dto.ModeloCambio;
        existente.Motivo = dto.Motivo;
        existente.ParPedido = dto.ParPedido;
        existente.DiferenciaAbonada = dto.DiferenciaAbonada;
        existente.DiferenciaAFavor = dto.DiferenciaAFavor;
        existente.Envio = dto.Envio;
        existente.Email = dto.Email;

        // Mantener los estados del DTO
        existente.LlegoAlDeposito = dto.LlegoAlDeposito;
        existente.YaEnviado = dto.YaEnviado;
        existente.CambioRegistradoSistema = dto.CambioRegistradoSistema;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActualizarEstadosAsync(int id, bool llegoAlDeposito, bool yaEnviado, bool cambioRegistradoSistema, bool parPedido)
    {
        var existente = await _context.Cambios.FindAsync(id);
        if (existente == null) return false;

        // Solo actualizar los estados
        existente.LlegoAlDeposito = llegoAlDeposito;
        existente.YaEnviado = yaEnviado;
        existente.CambioRegistradoSistema = cambioRegistradoSistema;
        existente.ParPedido = parPedido;

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