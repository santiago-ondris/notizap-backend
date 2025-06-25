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

    public async Task<List<PublicidadResumenMensualDto>> GetResumenMensualAsync(
        int year,
        int month,
        string unidadNegocio = null!)
    {
        var reports = _context.AdReports
            .Where(r => r.Year == year && r.Month == month);

        if (!string.IsNullOrWhiteSpace(unidadNegocio))
            reports = reports.Where(r => r.UnidadNegocio == unidadNegocio);

        var campañas = reports.SelectMany(r => r.Campañas);

        var montoTotal = await campañas.SumAsync(c => c.MontoInvertido);
        var totalClicks = await campañas.SumAsync(c => c.Clicks);
        var totalImpr = await campañas.SumAsync(c => c.Impressions);
        var totalReach = await campañas.SumAsync(c => c.Reach);
        var totalManualFol = await campañas.SumAsync(c => c.FollowersCount);
        var campaignCount = await campañas.CountAsync();

        return new List<PublicidadResumenMensualDto>
        {
            new PublicidadResumenMensualDto
            {
                UnidadNegocio      = string.IsNullOrWhiteSpace(unidadNegocio)
                                    ? "Todas"
                                    : unidadNegocio,
                MontoTotal         = montoTotal,
                TotalClicks        = totalClicks,
                TotalImpressions   = totalImpr,
                TotalReach         = totalReach,
                TotalManualFollowers = totalManualFol,
                CampaignCount      = campaignCount
            }
        };
    }

    public async Task<SyncResultDto> SyncReportFromApiAsync(
        string unidadNegocio,
        DateTime from,
        DateTime to)
    {
        const string plataforma = "Meta";
        var year  = from.Year;
        var month = from.Month;

        var report = await _context.AdReports
            .Include(r => r.Campañas)
            .FirstOrDefaultAsync(r =>
                r.UnidadNegocio == unidadNegocio &&
                r.Plataforma    == plataforma    &&
                r.Year          == year         &&
                r.Month         == month);

        var isNew = report == null;
        if (isNew)
        {
            report = new AdReport
            {
                UnidadNegocio = unidadNegocio,
                Plataforma    = plataforma,
                Year          = year,
                Month         = month,
                Campañas      = new List<AdCampaign>()
            };
            _context.AdReports.Add(report);
        }

        var accountId   = unidadNegocio.ToLower() switch
        {
            "montella" => "act_70862159",
            "alenka"   => "act_891129171656093",
            _ => throw new ArgumentException("Unidad no reconocida.")
        };
        var apiInsights = await _metaAds.GetCampaignInsightsAsync(accountId, from, to);

        var updated   = new List<string>();
        var unchanged = new List<string>();

        foreach (var ins in apiInsights)
        {
            var camp = report!.Campañas
                .FirstOrDefault(c => c.CampaignId == ins.CampaignId);

            if (camp == null)
            {
                var toAdd = _mapper.Map<AdCampaign>(ins);
                toAdd.FechaInicio = DateTime.SpecifyKind(ins.Start, DateTimeKind.Utc);
                toAdd.FechaFin    = DateTime.SpecifyKind(ins.End,   DateTimeKind.Utc);
                report.Campañas.Add(toAdd);
                updated.Add(ins.CampaignId);
            }
            else
            {
                // Compara y actualiza metricas
                bool hasChanged = false;
                if (camp.MontoInvertido != ins.Spend)        { camp.MontoInvertido = ins.Spend;        hasChanged = true; }
                if (camp.Clicks         != ins.Clicks)       { camp.Clicks         = ins.Clicks;       hasChanged = true; }
                if (camp.Impressions    != ins.Impressions)  { camp.Impressions    = ins.Impressions;  hasChanged = true; }
                if (camp.Ctr            != ins.Ctr)          { camp.Ctr            = ins.Ctr;          hasChanged = true; }
                if (camp.Reach          != ins.Reach)        { camp.Reach          = ins.Reach;        hasChanged = true; }
                if (camp.ValorResultado != ins.ValorResultado.ToString()) { camp.ValorResultado = ins.ValorResultado.ToString(); hasChanged = true; }
                if (camp.FechaInicio    != ins.Start)        { camp.FechaInicio    = ins.Start;        hasChanged = true; }
                if (camp.FechaFin       != ins.End)          { camp.FechaFin       = ins.End;          hasChanged = true; }

                if (camp.FechaInicio != ins.Start)
                {
                    camp.FechaInicio = DateTime.SpecifyKind(ins.Start, DateTimeKind.Utc);
                    hasChanged = true;
                }

                if (camp.FechaFin != ins.End)
                {
                    camp.FechaFin = DateTime.SpecifyKind(ins.End, DateTimeKind.Utc);
                    hasChanged = true;
                }

                if (hasChanged) updated.Add(ins.CampaignId);
                else            unchanged.Add(ins.CampaignId);
            }
        }

        await _context.SaveChangesAsync();

        return new SyncResultDto
        {
            ReportId         = report!.Id,
            UpdatedCampaigns = updated,
            UnchangedCampaigns = unchanged
        };
    }

    public async Task<PublicidadDashboardDto> GetDashboardDataAsync(PublicidadDashboardParamsDto parametros)
    {
        // Determinar fechas si no se proporcionan
        var fechaFin = parametros.FechaFin ?? DateTime.UtcNow;
        var fechaInicio = parametros.FechaInicio ?? new DateTime(fechaFin.Year, fechaFin.Month, 1);
        
        // Mes anterior para comparativas
        var fechaInicioAnterior = fechaInicio.AddMonths(-1);
        var fechaFinAnterior = fechaInicio.AddDays(-1);

        // Obtener TODOS los reportes del período actual (manuales + sincronizados)
        var reportesActuales = await _context.AdReports
            .Include(r => r.Campañas)
            .Where(r => r.Year == fechaFin.Year && r.Month == fechaFin.Month)
            .Where(r => !parametros.UnidadesNegocio.Any() || parametros.UnidadesNegocio.Contains(r.UnidadNegocio))
            .Where(r => !parametros.Plataformas.Any() || parametros.Plataformas.Contains(r.Plataforma))
            .ToListAsync();

        // Obtener TODOS los reportes del mes anterior (manuales + sincronizados)
        var reportesAnteriores = await _context.AdReports
            .Include(r => r.Campañas)
            .Where(r => r.Year == fechaInicioAnterior.Year && r.Month == fechaInicioAnterior.Month)
            .Where(r => !parametros.UnidadesNegocio.Any() || parametros.UnidadesNegocio.Contains(r.UnidadNegocio))
            .Where(r => !parametros.Plataformas.Any() || parametros.Plataformas.Contains(r.Plataforma))
            .ToListAsync();

        var dashboard = new PublicidadDashboardDto
        {
            ResumenGeneral = CalcularResumenGeneral(reportesActuales, reportesAnteriores),
            ResumenPorUnidad = PublicidadDashboardHelper.CalcularResumenPorUnidad(reportesActuales, reportesAnteriores),
            ResumenPorPlataforma = PublicidadDashboardHelper.CalcularResumenPorPlataforma(reportesActuales),
            Comparativa = PublicidadDashboardHelper.CalcularComparativa(reportesActuales, reportesAnteriores, fechaFin),
            TopCampañas = PublicidadDashboardHelper.ObtenerTopCampañas(reportesActuales, parametros.TopCampañasLimit),
            TendenciaMensual = await ObtenerTendenciaMensual(fechaFin, parametros.MesesHaciaAtras ?? 6, parametros.UnidadesNegocio, parametros.Plataformas)
        };

        return dashboard;
    }

    private PublicidadResumenGeneralDto CalcularResumenGeneral(
        List<AdReport> reportesActuales, 
        List<AdReport> reportesAnteriores)
    {
        // Totales actuales (todos los datos ya están en AdReports)
        var gastoTotalActual = reportesActuales.SelectMany(r => r.Campañas).Sum(c => c.MontoInvertido);
        var gastoTotalAnterior = reportesAnteriores.SelectMany(r => r.Campañas).Sum(c => c.MontoInvertido);

        // Métricas solo de campañas que tienen datos (las sincronizadas de Meta)
        var campañasConMetricas = reportesActuales.SelectMany(r => r.Campañas)
            .Where(c => c.Clicks > 0 || c.Impressions > 0); // Solo las que tienen métricas de Meta

        var totalClicks = campañasConMetricas.Sum(c => c.Clicks);
        var totalImpressions = campañasConMetricas.Sum(c => c.Impressions);
        var totalReach = campañasConMetricas.Sum(c => c.Reach);
        var ctrPromedio = totalImpressions > 0 ? (decimal)totalClicks / totalImpressions * 100 : 0;
        var costoPromedioPorClick = totalClicks > 0 ? gastoTotalActual / totalClicks : 0;

        var porcentajeCambio = gastoTotalAnterior > 0 ? 
            ((gastoTotalActual - gastoTotalAnterior) / gastoTotalAnterior) * 100 : 0;

        return new PublicidadResumenGeneralDto
        {
            GastoTotalActual = gastoTotalActual,
            GastoTotalAnterior = gastoTotalAnterior,
            PorcentajeCambio = Math.Round(porcentajeCambio, 2),
            TotalCampañasActivas = reportesActuales.SelectMany(r => r.Campañas).Count(),
            TotalClicks = totalClicks,
            TotalImpressions = totalImpressions,
            TotalReach = totalReach,
            CtrPromedio = Math.Round(ctrPromedio, 2),
            CostoPromedioPorClick = Math.Round(costoPromedioPorClick, 2)
        };
    }

  private async Task<List<TendenciaMensualDto>> ObtenerTendenciaMensual(
    DateTime fechaActual, 
    int mesesHaciaAtras,
    List<string> unidadesNegocio,
    List<string> plataformas)
    {
        var resultado = new List<TendenciaMensualDto>();
        var mesesNombres = new[] { "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", 
                                "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };

        // Generar lista de meses hacia atrás
        for (int i = mesesHaciaAtras - 1; i >= 0; i--)
        {
            var fechaMes = fechaActual.AddMonths(-i);
            var año = fechaMes.Year;
            var mes = fechaMes.Month;

            // Obtener reportes del mes específico
            var reportesMes = await _context.AdReports
                .Include(r => r.Campañas)
                .Where(r => r.Year == año && r.Month == mes)
                .Where(r => !unidadesNegocio.Any() || unidadesNegocio.Contains(r.UnidadNegocio))
                .Where(r => !plataformas.Any() || plataformas.Contains(r.Plataforma))
                .ToListAsync();

            if (!reportesMes.Any())
            {
                // Agregar mes con datos en 0 si no hay reportes
                resultado.Add(new TendenciaMensualDto
                {
                    Mes = mes,
                    Año = año,
                    MesNombre = mesesNombres[mes],
                    GastoTotal = 0,
                    GastoMontella = 0,
                    GastoAlenka = 0,
                    GastoKids = 0,
                    GastoMeta = 0,
                    GastoGoogle = 0,
                    TotalCampañas = 0
                });
                continue;
            }

            // Calcular totales del mes
            var gastoTotal = reportesMes.SelectMany(r => r.Campañas).Sum(c => c.MontoInvertido);
            var totalCampañas = reportesMes.SelectMany(r => r.Campañas).Count();

            // Gastos por unidad
            var gastoMontella = reportesMes
                .Where(r => r.UnidadNegocio.Equals("montella", StringComparison.OrdinalIgnoreCase))
                .SelectMany(r => r.Campañas)
                .Sum(c => c.MontoInvertido);

            var gastoAlenka = reportesMes
                .Where(r => r.UnidadNegocio.Equals("alenka", StringComparison.OrdinalIgnoreCase))
                .SelectMany(r => r.Campañas)
                .Sum(c => c.MontoInvertido);

            var gastoKids = reportesMes
                .Where(r => r.UnidadNegocio.Equals("kids", StringComparison.OrdinalIgnoreCase))
                .SelectMany(r => r.Campañas)
                .Sum(c => c.MontoInvertido);

            // Gastos por plataforma
            var gastoMeta = reportesMes
                .Where(r => r.Plataforma.Equals("Meta", StringComparison.OrdinalIgnoreCase))
                .SelectMany(r => r.Campañas)
                .Sum(c => c.MontoInvertido);

            var gastoGoogle = reportesMes
                .Where(r => r.Plataforma.Equals("Google", StringComparison.OrdinalIgnoreCase))
                .SelectMany(r => r.Campañas)
                .Sum(c => c.MontoInvertido);

            resultado.Add(new TendenciaMensualDto
            {
                Mes = mes,
                Año = año,
                MesNombre = mesesNombres[mes],
                GastoTotal = gastoTotal,
                GastoMontella = gastoMontella,
                GastoAlenka = gastoAlenka,
                GastoKids = gastoKids,
                GastoMeta = gastoMeta,
                GastoGoogle = gastoGoogle,
                TotalCampañas = totalCampañas
            });
        }

        return resultado.OrderBy(r => r.Año).ThenBy(r => r.Mes).ToList();
    }
}
