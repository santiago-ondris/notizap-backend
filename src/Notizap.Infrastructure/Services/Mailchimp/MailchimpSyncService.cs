using Microsoft.EntityFrameworkCore;

public class MailchimpSyncService : IMailchimpSyncService
{
    private readonly NotizapDbContext _context;
    private readonly IMailchimpServiceFactory _factory;

    public MailchimpSyncService(NotizapDbContext context, IMailchimpServiceFactory factory)
    {
        _context = context;
        _factory = factory;
    }

    public async Task<MailchimpSyncResultDto> SyncAsync(string cuenta)
    {
        var service = _factory.Create(cuenta);
        var campaigns = await service.GetAvailableCampaignsAsync();

        int nuevas = 0;
        int actualizadas = 0;

        foreach (var campaign in campaigns)
        {
            // Buscar si la campaña ya existe en la DB
            var existingCampaign = await _context.CampaignMailchimps
                .FirstOrDefaultAsync(c => c.CampaignId == campaign.Id && c.Cuenta == cuenta);

            // Obtener las métricas actualizadas desde la API
            var stats = await service.GetCampaignStatsAsync(campaign.Id);

            if (existingCampaign == null)
            {
                // Campaña nueva - agregar
                _context.CampaignMailchimps.Add(new CampaignMailchimp
                {
                    CampaignId = campaign.Id,
                    Title = stats.CampaignTitle,
                    SendTime = stats.SendTime.ToUniversalTime(),
                    EmailsSent = stats.EmailsSent,
                    OpenRate = stats.OpenRate,
                    ClickRate = stats.ClickRate,
                    Conversions = stats.Conversions,
                    Cuenta = cuenta
                });
                nuevas++;
            }
            else
            {
                // Campaña existente - actualizar métricas
                existingCampaign.Title = stats.CampaignTitle;
                existingCampaign.SendTime = stats.SendTime.ToUniversalTime();
                existingCampaign.EmailsSent = stats.EmailsSent;
                existingCampaign.OpenRate = stats.OpenRate;
                existingCampaign.ClickRate = stats.ClickRate;
                existingCampaign.Conversions = stats.Conversions;
                
                _context.CampaignMailchimps.Update(existingCampaign);
                actualizadas++;
            }
        }

        await _context.SaveChangesAsync();

        return new MailchimpSyncResultDto
        {
            NuevasCampañas = nuevas,
            CampañasActualizadas = actualizadas,
            TotalProcesadas = campaigns.Count,
            Mensaje = GenerarMensaje(nuevas, actualizadas)
        };
    }

    private static string GenerarMensaje(int nuevas, int actualizadas)
    {
        if (nuevas == 0 && actualizadas == 0)
            return "No se encontraron cambios para sincronizar.";

        var partes = new List<string>();
        
        if (nuevas > 0)
            partes.Add($"{nuevas} campaña{(nuevas > 1 ? "s" : "")} nueva{(nuevas > 1 ? "s" : "")} agregada{(nuevas > 1 ? "s" : "")}");
        
        if (actualizadas > 0)
            partes.Add($"{actualizadas} campaña{(actualizadas > 1 ? "s" : "")} actualizada{(actualizadas > 1 ? "s" : "")}");

        return string.Join(" y ", partes) + ".";
    }

    public Task<List<CampaignMailchimp>> GetAllAsync(string cuenta)
    {
        return _context.CampaignMailchimps
            .Where(c => c.Cuenta == cuenta)
            .OrderByDescending(c => c.SendTime)
            .ToListAsync();
    }
}