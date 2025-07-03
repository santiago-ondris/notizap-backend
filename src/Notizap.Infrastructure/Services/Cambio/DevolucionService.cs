using AutoMapper;
using Microsoft.EntityFrameworkCore;

public class DevolucionService : IDevolucionService
{
    private readonly NotizapDbContext _context;
    private readonly IMapper _mapper;

    public DevolucionService(NotizapDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<int> CrearDevolucionAsync(CreateDevolucionDto dto)
    {
        var devolucion = _mapper.Map<Devolucion>(dto);
        
        devolucion.Fecha = DateTime.SpecifyKind(devolucion.Fecha, DateTimeKind.Utc);
        
        // Setear estados iniciales
        devolucion.LlegoAlDeposito = false;
        devolucion.DineroDevuelto = false;
        devolucion.NotaCreditoEmitida = false;

        _context.Devoluciones.Add(devolucion);
        await _context.SaveChangesAsync();
        return devolucion.Id;
    }

    public async Task<List<DevolucionDto>> ObtenerTodasAsync()
    {
        var devoluciones = await _context.Devoluciones
            .OrderByDescending(d => d.Fecha)
            .ToListAsync();

        return _mapper.Map<List<DevolucionDto>>(devoluciones);
    }

    public async Task<DevolucionDto?> ObtenerPorIdAsync(int id)
    {
        var devolucion = await _context.Devoluciones.FindAsync(id);
        return devolucion == null ? null : _mapper.Map<DevolucionDto>(devolucion);
    }

    public async Task<bool> ActualizarDevolucionAsync(int id, DevolucionDto dto)
    {
        var existente = await _context.Devoluciones.FindAsync(id);
        if (existente == null) return false;

        // Mapear datos del DTO manualmente para controlar la fecha
        existente.Fecha = DateTime.SpecifyKind(dto.Fecha, DateTimeKind.Utc);
        existente.Pedido = dto.Pedido;
        existente.Celular = dto.Celular;
        existente.Modelo = dto.Modelo;
        existente.Motivo = dto.Motivo;
        existente.Monto = dto.Monto;
        existente.PagoEnvio = dto.PagoEnvio;
        existente.Responsable = dto.Responsable;
        
        // Mantener los estados del DTO
        existente.LlegoAlDeposito = dto.LlegoAlDeposito;
        existente.DineroDevuelto = dto.DineroDevuelto;
        existente.NotaCreditoEmitida = dto.NotaCreditoEmitida;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarDevolucionAsync(int id)
    {
        var devolucion = await _context.Devoluciones.FindAsync(id);
        if (devolucion == null) return false;

        _context.Devoluciones.Remove(devolucion);
        await _context.SaveChangesAsync();
        return true;
    }
}