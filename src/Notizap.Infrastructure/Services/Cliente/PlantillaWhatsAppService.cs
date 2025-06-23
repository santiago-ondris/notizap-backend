using Microsoft.EntityFrameworkCore;

public class PlantillaWhatsAppService : IPlantillaWhatsAppService
{
    private readonly NotizapDbContext _context;

    public PlantillaWhatsAppService(NotizapDbContext context)
    {
        _context = context;
    }

    public async Task<List<PlantillaWhatsAppDto>> GetAllActivasAsync()
    {
        return await _context.PlantillasWhatsApp
            .Where(p => p.Activa)
            .OrderBy(p => p.Categoria)
            .ThenBy(p => p.Nombre)
            .Select(p => new PlantillaWhatsAppDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Mensaje = p.Mensaje,
                Descripcion = p.Descripcion,
                Categoria = p.Categoria,
                FechaCreacion = p.FechaCreacion,
                CreadoPor = p.CreadoPor,
                Activa = p.Activa
            })
            .ToListAsync();
    }

    public async Task<Dictionary<string, List<PlantillaWhatsAppDto>>> GetPorCategoriasAsync()
    {
        var plantillas = await GetAllActivasAsync();
        
        return plantillas
            .GroupBy(p => p.Categoria)
            .ToDictionary(
                g => g.Key,
                g => g.ToList()
            );
    }

    public async Task<PlantillaWhatsAppDto?> GetByIdAsync(int id)
    {
        var plantilla = await _context.PlantillasWhatsApp
            .Where(p => p.Id == id && p.Activa)
            .FirstOrDefaultAsync();

        if (plantilla == null) return null;

        return new PlantillaWhatsAppDto
        {
            Id = plantilla.Id,
            Nombre = plantilla.Nombre,
            Mensaje = plantilla.Mensaje,
            Descripcion = plantilla.Descripcion,
            Categoria = plantilla.Categoria,
            FechaCreacion = plantilla.FechaCreacion,
            CreadoPor = plantilla.CreadoPor,
            Activa = plantilla.Activa
        };
    }

    public async Task<PlantillaWhatsAppDto> CrearAsync(CrearPlantillaWhatsAppDto dto, string username)
    {
        var plantilla = new PlantillaWhatsApp
        {
            Nombre = dto.Nombre,
            Mensaje = dto.Mensaje,
            Descripcion = dto.Descripcion,
            Categoria = dto.Categoria,
            CreadoPor = username,
            FechaCreacion = DateTime.UtcNow,
            Activa = true
        };

        _context.PlantillasWhatsApp.Add(plantilla);
        await _context.SaveChangesAsync();

        return new PlantillaWhatsAppDto
        {
            Id = plantilla.Id,
            Nombre = plantilla.Nombre,
            Mensaje = plantilla.Mensaje,
            Descripcion = plantilla.Descripcion,
            Categoria = plantilla.Categoria,
            FechaCreacion = plantilla.FechaCreacion,
            CreadoPor = plantilla.CreadoPor,
            Activa = plantilla.Activa
        };
    }

    public async Task<PlantillaWhatsAppDto?> ActualizarAsync(int id, ActualizarPlantillaWhatsAppDto dto)
    {
        var plantilla = await _context.PlantillasWhatsApp.FindAsync(id);
        if (plantilla == null) return null;

        plantilla.Nombre = dto.Nombre;
        plantilla.Mensaje = dto.Mensaje;
        plantilla.Descripcion = dto.Descripcion;
        plantilla.Categoria = dto.Categoria;
        plantilla.Activa = dto.Activa;

        await _context.SaveChangesAsync();

        return new PlantillaWhatsAppDto
        {
            Id = plantilla.Id,
            Nombre = plantilla.Nombre,
            Mensaje = plantilla.Mensaje,
            Descripcion = plantilla.Descripcion,
            Categoria = plantilla.Categoria,
            FechaCreacion = plantilla.FechaCreacion,
            CreadoPor = plantilla.CreadoPor,
            Activa = plantilla.Activa
        };
    }

    public async Task<bool> DesactivarAsync(int id)
    {
        var plantilla = await _context.PlantillasWhatsApp.FindAsync(id);
        if (plantilla == null) return false;

        plantilla.Activa = false;
        await _context.SaveChangesAsync();
        
        return true;
    }
}