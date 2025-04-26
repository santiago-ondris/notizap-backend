public interface IMailchimpService
{
  Task<MailchimpStatsDto> GetCampaignStatsAsync(string? campaignId = null);
  Task<List<MailchimpCampaignDto>> GetAvailableCampaignsAsync();
  Task<MailchimpHighlightsDto> GetHighlightsAsync();
}