public interface IMailchimpService
{
    Task<List<MailchimpCampaignDto>> GetAvailableCampaignsAsync();
    Task<MailchimpStatsDto> GetCampaignStatsAsync(string? campaignId = null);
}