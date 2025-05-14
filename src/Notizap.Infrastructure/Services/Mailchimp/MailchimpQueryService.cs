using Microsoft.EntityFrameworkCore;

public class MailchimpQueryService : IMailchimpQueryService
{
    private readonly NotizapDbContext _context;

    public MailchimpQueryService(NotizapDbContext context)
    {
        _context = context;
    }

    public Task<List<CampaignMailchimp>> GetAllCampaignsAsync(string cuenta)
    {
        return _context.CampaignMailchimps
            .Where(c => c.Cuenta == cuenta)
            .OrderByDescending(c => c.SendTime)
            .ToListAsync();
    }

    public async Task<MailchimpStatsDto?> GetStatsByCampaignIdAsync(string campaignId)
    {
        var campaign = await _context.CampaignMailchimps
            .FirstOrDefaultAsync(c => c.CampaignId == campaignId);

        if (campaign is null) return null;

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

    public async Task<MailchimpHighlightsDto> GetHighlightsAsync(string cuenta)
    {
        var campaigns = await _context.CampaignMailchimps
            .Where(c => c.Cuenta == cuenta)
            .ToListAsync();

        if (!campaigns.Any())
            return new MailchimpHighlightsDto
            {
                BestOpenRateCampaign = "Sin datos",
                BestClickRateCampaign = "Sin datos",
                BestConversionCampaign = "Sin datos",
                BestOpenRate = 0,
                BestClickRate = 0,
                BestConversions = 0
            };

        var bestOpen = campaigns.OrderByDescending(c => c.OpenRate).First();
        var bestClick = campaigns.OrderByDescending(c => c.ClickRate).First();
        var bestConv = campaigns.OrderByDescending(c => c.Conversions).First();

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
}
