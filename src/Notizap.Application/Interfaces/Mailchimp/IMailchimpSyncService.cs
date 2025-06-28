public interface IMailchimpSyncService
{
    Task<MailchimpSyncResultDto> SyncAsync(string cuenta);
    Task<List<CampaignMailchimp>> GetAllAsync(string cuenta);
}