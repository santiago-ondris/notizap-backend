using AutoMapper;
using Microsoft.EntityFrameworkCore;

public class AdService : IAdService
{
    private readonly NotizapDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMetaAdsService _metaAds;

    public AdService(NotizapDbContext context, IMapper mapper, IMetaAdsService metaAds)
    {
        _context = context;
        _mapper = mapper;
        _metaAds = metaAds;
    }

    public async Task<List<AdReportDto>> GetAllAsync()
    {
        var reports = await _context.AdReports
            .Include(r => r.Campañas)
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Month)
            .ToListAsync();

        return _mapper.Map<List<AdReportDto>>(reports);
    }

    public async Task<AdReportDto?> GetByIdAsync(int id)
    {
        var report = await _context.AdReports
            .Include(r => r.Campañas)
            .FirstOrDefaultAsync(r => r.Id == id);

        return report is null ? null : _mapper.Map<AdReportDto>(report);
    }

    public async Task<AdReportDto> CreateAsync(SaveAdReportDto dto)
    {
        var report = _mapper.Map<AdReport>(dto);

        foreach (var camp in report.Campañas)
        {
            camp.FechaInicio = DateTime.SpecifyKind(camp.FechaInicio, DateTimeKind.Utc);
            camp.FechaFin    = DateTime.SpecifyKind(camp.FechaFin,    DateTimeKind.Utc);
        }

        _context.AdReports.Add(report);
        await _context.SaveChangesAsync();

        return _mapper.Map<AdReportDto>(report);
    }

    public async Task<AdReportDto?> UpdateAsync(int id, SaveAdReportDto dto)
    {
        var existing = await _context.AdReports
            .Include(r => r.Campañas)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (existing is null)
            return null;

        existing.UnidadNegocio = dto.UnidadNegocio;
        existing.Plataforma = dto.Plataforma;
        existing.Year = dto.Year;
        existing.Month = dto.Month;

        foreach (var camp in existing.Campañas)
        {
            camp.FechaInicio = DateTime.SpecifyKind(camp.FechaInicio, DateTimeKind.Utc);
            camp.FechaFin    = DateTime.SpecifyKind(camp.FechaFin,    DateTimeKind.Utc);
        }

        _context.AdCampaigns.RemoveRange(existing.Campañas);
        existing.Campañas = _mapper.Map<List<AdCampaign>>(dto.Campañas);

        await _context.SaveChangesAsync();
        return _mapper.Map<AdReportDto>(existing);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var report = await _context.AdReports.FindAsync(id);
        if (report is null) return false;

        _context.AdReports.Remove(report);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<PublicidadResumenMensualDto>> GetResumenMensualAsync(int year, int month)
    {
        return await _context.AdReports
            .Where(r => r.Year == year && r.Month == month)
            .SelectMany(r => r.Campañas, (reporte, campaña) => new { reporte, campaña })
            .GroupBy(x => new { x.reporte.UnidadNegocio, x.reporte.Plataforma })
            .Select(g => new PublicidadResumenMensualDto
            {
                UnidadNegocio = g.Key.UnidadNegocio,
                Plataforma = g.Key.Plataforma,
                MontoTotal = g.Sum(x => x.campaña.MontoInvertido)
            })
            .ToListAsync();
    }

    public async Task<SyncResultDto> SyncReportFromApiAsync(
        int reportId,
        string adAccountId,
        DateTime from,
        DateTime to)
    {
        var report = await _context.AdReports
            .Include(r => r.Campañas)
            .FirstOrDefaultAsync(r => r.Id == reportId);

        if (report == null)
            throw new KeyNotFoundException($"Reporte {reportId} no existe");

        var apiInsights = await _metaAds.GetCampaignInsightsAsync(adAccountId, from, to);

        var updated   = new List<string>();
        var unchanged = new List<string>();

        foreach (var ins in apiInsights)
        {
            var camp = report.Campañas
                .FirstOrDefault(c => c.CampaignId == ins.CampaignId);

            if (camp == null)
            {
                var toAdd = _mapper.Map<AdCampaign>(ins);
                report.Campañas.Add(toAdd);
                updated.Add(ins.CampaignId);
            }
            else
            {
                // Comparar métrica a métrica
                bool hasChanged = false;

                if (camp.MontoInvertido  != ins.Spend)       { camp.MontoInvertido  = ins.Spend;       hasChanged = true; }
                if (camp.Clicks          != ins.Clicks)      { camp.Clicks          = ins.Clicks;      hasChanged = true; }
                if (camp.Impressions     != ins.Impressions) { camp.Impressions     = ins.Impressions; hasChanged = true; }
                if (camp.Ctr             != ins.Ctr)         { camp.Ctr             = ins.Ctr;         hasChanged = true; }
                if (camp.Reach           != ins.Reach)       { camp.Reach           = ins.Reach;       hasChanged = true; }
                if (camp.ValorResultado  != ins.ValorResultado) { camp.ValorResultado = ins.ValorResultado; hasChanged = true; }
                if (camp.FechaInicio     != ins.Start)       { camp.FechaInicio     = ins.Start;       hasChanged = true; }
                if (camp.FechaFin        != ins.End)         { camp.FechaFin        = ins.End;         hasChanged = true; }

                if (hasChanged)
                    updated.Add(ins.CampaignId);
                else
                    unchanged.Add(ins.CampaignId);
            }
        }

        await _context.SaveChangesAsync();
        return new SyncResultDto { UpdatedCampaigns = updated, UnchangedCampaigns = unchanged };
    }
}
