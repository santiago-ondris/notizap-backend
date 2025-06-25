using AutoMapper;
using Microsoft.EntityFrameworkCore;

public class UpdateAdService : IUpdateAdService
{
    private readonly NotizapDbContext _context;
    private readonly IMapper _mapper;

    public UpdateAdService(NotizapDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<AdCampaignReadDto>> GetAllCampaignsAsync(
        string? unidad = null,
        string? plataforma = null,
        int? year = null,
        int? month = null,
        int page = 1,
        int pageSize = 20)
    {
        var query = _context.AdCampaigns
            .Include(c => c.Reporte)
            .AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(unidad))
        {
            query = query.Where(c => c.Reporte.UnidadNegocio.ToLower() == unidad.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(plataforma))
        {
            query = query.Where(c => c.Reporte.Plataforma.ToLower() == plataforma.ToLower());
        }

        if (year.HasValue)
        {
            query = query.Where(c => c.Reporte.Year == year.Value);
        }

        if (month.HasValue)
        {
            query = query.Where(c => c.Reporte.Month == month.Value);
        }

        // Ordenar por fecha más reciente primero
        query = query.OrderByDescending(c => c.Reporte.Year)
                     .ThenByDescending(c => c.Reporte.Month)
                     .ThenByDescending(c => c.FechaInicio);

        // Aplicar paginación
        var campaigns = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Mapear a DTOs con información adicional del reporte
        var result = new List<AdCampaignReadDto>();
        foreach (var campaign in campaigns)
        {
            var dto = _mapper.Map<AdCampaignReadDto>(campaign);
            dto.UnidadNegocio = campaign.Reporte.UnidadNegocio;
            dto.Plataforma = campaign.Reporte.Plataforma;
            dto.Year = campaign.Reporte.Year;
            dto.Month = campaign.Reporte.Month;
            result.Add(dto);
        }

        return result;
    }

    public async Task<AdCampaignReadDto?> GetCampaignByIdAsync(int id)
    {
        var campaign = await _context.AdCampaigns
            .Include(c => c.Reporte)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign == null)
            return null;

        return _mapper.Map<AdCampaignReadDto>(campaign);
    }

    public async Task<AdCampaignReadDto?> UpdateCampaignAsync(int id, UpdateAdCampaignDto dto)
    {
        var existingCampaign = await _context.AdCampaigns
            .Include(c => c.Reporte)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (existingCampaign == null)
            return null;

        // Verificar que no exista otra campaña con el mismo CampaignId en el mismo reporte
        var duplicateCampaign = await _context.AdCampaigns
            .Where(c => c.Id != id && 
                       c.CampaignId == dto.CampaignId && 
                       c.AdReportId == existingCampaign.AdReportId)
            .FirstOrDefaultAsync();

        if (duplicateCampaign != null)
        {
            throw new InvalidOperationException($"Ya existe una campaña con el ID '{dto.CampaignId}' en este reporte.");
        }

        // Actualizar todos los campos editables
        existingCampaign.CampaignId = dto.CampaignId;
        existingCampaign.Nombre = dto.Nombre;
        existingCampaign.Tipo = dto.Tipo;
        existingCampaign.MontoInvertido = dto.MontoInvertido;
        existingCampaign.Objetivo = dto.Objetivo;
        existingCampaign.Resultados = dto.Resultados;
        existingCampaign.FollowersCount = dto.FollowersCount;

        // Manejar fechas con UTC
        existingCampaign.FechaInicio = DateTime.SpecifyKind(dto.FechaInicio, DateTimeKind.Utc);
        existingCampaign.FechaFin = DateTime.SpecifyKind(dto.FechaFin, DateTimeKind.Utc);

        // Actualizar métricas si se proporcionan (para campañas de API)
        if (dto.Clicks.HasValue)
            existingCampaign.Clicks = dto.Clicks.Value;

        if (dto.Impressions.HasValue)
            existingCampaign.Impressions = dto.Impressions.Value;

        if (dto.Ctr.HasValue)
            existingCampaign.Ctr = dto.Ctr.Value;

        if (dto.Reach.HasValue)
            existingCampaign.Reach = dto.Reach.Value;

        if (!string.IsNullOrEmpty(dto.ValorResultado))
            existingCampaign.ValorResultado = dto.ValorResultado;

        try
        {
            await _context.SaveChangesAsync();
            return _mapper.Map<AdCampaignReadDto>(existingCampaign);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Error al actualizar la campaña. Verifica que los datos sean válidos.");
        }
    }

    public async Task<int> GetTotalCampaignsCountAsync(
        string? unidad = null,
        string? plataforma = null,
        int? year = null,
        int? month = null)
    {
        var query = _context.AdCampaigns
            .Include(c => c.Reporte)
            .AsQueryable();

        // Aplicar los mismos filtros que en GetAllCampaignsAsync
        if (!string.IsNullOrWhiteSpace(unidad))
        {
            query = query.Where(c => c.Reporte.UnidadNegocio.ToLower() == unidad.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(plataforma))
        {
            query = query.Where(c => c.Reporte.Plataforma.ToLower() == plataforma.ToLower());
        }

        if (year.HasValue)
        {
            query = query.Where(c => c.Reporte.Year == year.Value);
        }

        if (month.HasValue)
        {
            query = query.Where(c => c.Reporte.Month == month.Value);
        }

        return await query.CountAsync();
    }
}