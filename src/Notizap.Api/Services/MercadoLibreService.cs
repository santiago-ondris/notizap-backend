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
    public async Task<List<DailySalesDto>> GetSimulatedDailyStatsAsync(int year, int month)
    {
        var report = await _context.MercadoLibreManualReports
            .FirstOrDefaultAsync(r => r.Year == year && r.Month == month);

        if (report == null)
            return new List<DailySalesDto>();

        int days = DateTime.DaysInMonth(year, month);
        decimal dailyRevenue = report.Revenue / days;
        int dailyUnits = report.UnitsSold / days;
        int remainder = report.UnitsSold % days;

        var dailyStats = new List<DailySalesDto>();

        for (int i = 1; i <= days; i++)
        {
            int units = dailyUnits + (i <= remainder ? 1 : 0); // repartir sobrantes
            dailyStats.Add(new DailySalesDto
            {
                Date = new DateTime(year, month, i),
                UnitsSold = units,
                Revenue = dailyRevenue
            });
        }

        return dailyStats;
    }
}
