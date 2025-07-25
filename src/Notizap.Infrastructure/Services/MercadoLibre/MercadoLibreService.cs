using Microsoft.EntityFrameworkCore;

public class MercadoLibreService : IMercadoLibreService
{
    private readonly NotizapDbContext _context;

    public MercadoLibreService(NotizapDbContext context)
    {
        _context = context;
    }

    public async Task<List<MercadoLibreManualReport>> GetAllAsync()
    {
        return await _context.MercadoLibreManualReports
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ToListAsync();
    }

    public async Task<MercadoLibreManualReport?> GetByIdAsync(int id)
    {
        return await _context.MercadoLibreManualReports.FindAsync(id);
    }

    public async Task<MercadoLibreManualReport> CreateAsync(MercadoLibreManualDto dto)
    {
        var existing = await _context.MercadoLibreManualReports
            .FirstOrDefaultAsync(r => r.Year == dto.Year && r.Month == dto.Month);

        if (existing != null)
        {
            existing.UnitsSold = dto.UnitsSold;
            existing.Revenue = dto.Revenue;
        }
        else
        {
            existing = new MercadoLibreManualReport
            {
                Year = dto.Year,
                Month = dto.Month,
                UnitsSold = dto.UnitsSold,
                Revenue = dto.Revenue
            };
            _context.MercadoLibreManualReports.Add(existing);
        }

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<MercadoLibreManualReport?> UpdateAsync(int id, MercadoLibreManualDto dto)
    {
        var existing = await _context.MercadoLibreManualReports.FindAsync(id);
        if (existing == null) return null;

        existing.Year = dto.Year;
        existing.Month = dto.Month;
        existing.UnitsSold = dto.UnitsSold;
        existing.Revenue = dto.Revenue;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.MercadoLibreManualReports.FindAsync(id);
        if (existing == null) return false;

        _context.MercadoLibreManualReports.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}
