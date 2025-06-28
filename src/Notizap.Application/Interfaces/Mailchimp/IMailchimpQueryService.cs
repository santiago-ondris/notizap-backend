public interface IMailchimpQueryService
{
    Task<List<CampaignMailchimp>> GetAllCampaignsAsync(string cuenta);
    Task<MailchimpStatsDto?> GetStatsByCampaignIdAsync(string campaignId);
    Task<MailchimpHighlightsDto> GetHighlightsAsync(string cuenta);
    Task<bool> UpdateCampaignTitleAsync(int campaignId, string newTitle);
}