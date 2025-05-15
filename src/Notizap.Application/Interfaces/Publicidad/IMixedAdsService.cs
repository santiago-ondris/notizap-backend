using Notizap.Application.Ads.Dtos;

namespace Notizap.Application.Ads.Services
{
    public interface IMixedAdsService
    {
        Task<List<MixedCampaignInsightDto>> 
            GetMixedCampaignInsightsAsync(
                string adAccountId,
                DateTime from,
                DateTime to,
                int manualReportId);
    }
}
