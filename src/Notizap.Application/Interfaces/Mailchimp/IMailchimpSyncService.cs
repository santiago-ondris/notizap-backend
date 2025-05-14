public interface IMailchimpSyncService
{
    Task<int> SyncAsync(string cuenta);
    Task<List<CampaignMailchimp>> GetAllAsync(string cuenta);
}
