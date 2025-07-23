using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class MailchimpQueryService : IMailchimpQueryService
{
    private readonly NotizapDbContext _context;
    private readonly ILogger<MailchimpQueryService> _logger;

    public MailchimpQueryService(NotizapDbContext context, ILogger<MailchimpQueryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<CampaignMailchimp>> GetAllCampaignsAsync(string cuenta)
    {
        _logger.LogInformation("📋 Obteniendo todas las campañas para cuenta {Cuenta}", cuenta);

        try
        {
            var campaigns = await _context.CampaignMailchimps
                .Where(c => c.Cuenta == cuenta)
                .OrderByDescending(c => c.SendTime)
                .ToListAsync();

            _logger.LogInformation("📊 Se obtuvieron {CantidadCampanas} campañas para cuenta {Cuenta}", 
                campaigns.Count, cuenta);

            // Log adicional si no hay campañas
            if (!campaigns.Any())
            {
                _logger.LogWarning("⚠️ No se encontraron campañas para la cuenta {Cuenta}", cuenta);
            }

            return campaigns;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener campañas para cuenta {Cuenta}", cuenta);
            throw;
        }
    }

    public async Task<MailchimpStatsDto?> GetStatsByCampaignIdAsync(string campaignId)
    {
        _logger.LogInformation("📈 Obteniendo estadísticas para campaña {CampaignId}", campaignId);

        try
        {
            var campaign = await _context.CampaignMailchimps
                .FirstOrDefaultAsync(c => c.CampaignId == campaignId);

            if (campaign is null)
            {
                _logger.LogWarning("⚠️ No se encontró campaña con ID {CampaignId}", campaignId);
                return null;
            }

            _logger.LogInformation("✅ Estadísticas obtenidas para campaña {CampaignId} - {Title}. OpenRate: {OpenRate:F2}%, ClickRate: {ClickRate:F2}%", 
                campaignId, campaign.Title, campaign.OpenRate, campaign.ClickRate);

            return new MailchimpStatsDto
            {
                CampaignTitle = campaign.Title,
                SendTime = campaign.SendTime,
                EmailsSent = campaign.EmailsSent,
                OpenRate = campaign.OpenRate,
                ClickRate = campaign.ClickRate,
                Conversions = campaign.Conversions
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener estadísticas para campaña {CampaignId}", campaignId);
            throw;
        }
    }

    public async Task<MailchimpHighlightsDto> GetHighlightsAsync(string cuenta)
    {
        _logger.LogInformation("🏆 Obteniendo highlights (mejores campañas) para cuenta {Cuenta}", cuenta);

        try
        {
            var campaigns = await _context.CampaignMailchimps
                .Where(c => c.Cuenta == cuenta)
                .ToListAsync();

            if (!campaigns.Any())
            {
                _logger.LogWarning("⚠️ No hay campañas disponibles para generar highlights en cuenta {Cuenta}", cuenta);
                
                return new MailchimpHighlightsDto
                {
                    BestOpenRateCampaign = "Sin datos",
                    BestClickRateCampaign = "Sin datos",
                    BestConversionCampaign = "Sin datos",
                    BestOpenRate = 0,
                    BestClickRate = 0,
                    BestConversions = 0
                };
            }

            var bestOpen = campaigns.OrderByDescending(c => c.OpenRate).First();
            var bestClick = campaigns.OrderByDescending(c => c.ClickRate).First();
            var bestConv = campaigns.OrderByDescending(c => c.Conversions).First();

            _logger.LogInformation("🎯 Highlights calculados para cuenta {Cuenta}: Mejor OpenRate: {BestOpenRate:F2}% ({BestOpenTitle}), Mejor ClickRate: {BestClickRate:F2}% ({BestClickTitle}), Mejores Conversiones: {BestConversions} ({BestConvTitle})",
                cuenta, bestOpen.OpenRate, bestOpen.Title, bestClick.ClickRate, bestClick.Title, bestConv.Conversions, bestConv.Title);

            return new MailchimpHighlightsDto
            {
                BestOpenRateCampaign = bestOpen.Title,
                BestOpenRate = bestOpen.OpenRate,
                BestClickRateCampaign = bestClick.Title,
                BestClickRate = bestClick.ClickRate,
                BestConversionCampaign = bestConv.Title,
                BestConversions = bestConv.Conversions
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al calcular highlights para cuenta {Cuenta}", cuenta);
            throw;
        }
    }

    public async Task<bool> UpdateCampaignTitleAsync(int campaignId, string newTitle)
    {
        _logger.LogInformation("✏️ Actualizando título de campaña ID {CampaignId} a '{NewTitle}'", campaignId, newTitle);

        try
        {
            var campaign = await _context.CampaignMailchimps
                .FirstOrDefaultAsync(c => c.Id == campaignId);

            if (campaign is null)
            {
                _logger.LogWarning("⚠️ No se encontró campaña con ID {CampaignId} para actualizar título", campaignId);
                return false;
            }

            var tituloAnterior = campaign.Title;
            campaign.Title = newTitle.Trim();
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Título de campaña ID {CampaignId} actualizado correctamente: '{TituloAnterior}' → '{NewTitle}'", 
                campaignId, tituloAnterior, newTitle.Trim());
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al actualizar título de campaña ID {CampaignId}", campaignId);
            throw;
        }
    }
}