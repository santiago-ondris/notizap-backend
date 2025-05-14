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

    public async Task<int> SyncAsync(string cuenta)
    {
        var service = _factory.Create(cuenta);
        var campaigns = await service.GetAvailableCampaignsAsync();

        int nuevas = 0;

        foreach (var campaign in campaigns)
        {
            var exists = await _context.CampaignMailchimps
                .AnyAsync(c => c.CampaignId == campaign.Id && c.Cuenta == cuenta);

            if (!exists)
            {
                var stats = await service.GetCampaignStatsAsync(campaign.Id);

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
        }

        await _context.SaveChangesAsync();
        return nuevas;
    }

    public Task<List<CampaignMailchimp>> GetAllAsync(string cuenta)
    {
        return _context.CampaignMailchimps
            .Where(c => c.Cuenta == cuenta)
            .OrderByDescending(c => c.SendTime)
            .ToListAsync();
    }
}
