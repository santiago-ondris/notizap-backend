public interface IMetaAdsService
{
    Task<List<MetaCampaignInsightDto>> GetCampaignInsightsAsync(string adAccountId, DateTime from, DateTime to);
}