using AutoMapper;
using Microsoft.EntityFrameworkCore;

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
            _context.Gastos.Add(gasto);
            await _context.SaveChangesAsync();
            return _mapper.Map<GastoDto>(gasto);
        }

        public async Task<GastoDto?> ActualizarAsync(int id, UpdateGastoDto dto)
        {
            var gasto = await _context.Gastos.FindAsync(id);
            if (gasto is null) return null;

            _mapper.Map(dto, gasto);
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
    }
}
