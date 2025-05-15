// /Infrastructure/Ads/MixedAdsService.cs
using AutoMapper;
using Notizap.Application.Ads.Dtos;
using Notizap.Application.Ads.Services;

namespace Notizap.Infrastructure.Ads
{
    public class MixedAdsService : IMixedAdsService
    {
        private readonly IMetaAdsService _metaAds;
        private readonly IAdService _adService;
        private readonly IMapper _mapper;

        public MixedAdsService(
            IMetaAdsService metaAds,
            IAdService adService,
            IMapper mapper)
        {
            _metaAds  = metaAds;
            _adService = adService;
            _mapper   = mapper;
        }

        public async Task<List<MixedCampaignInsightDto>> 
            GetMixedCampaignInsightsAsync(
                string adAccountId,
                DateTime from,
                DateTime to,
                int manualReportId)
        {

            var apiInsights = await _metaAds.GetCampaignInsightsAsync(adAccountId, from, to);

            var manualReport = await _adService.GetByIdAsync(manualReportId);
            var manualMap = manualReport!.Campañas
                .ToDictionary(c => c.CampaignId, c => c.FollowersCount);

            var mixed = apiInsights
                .Select(ins =>
                {
                    var dto = _mapper.Map<MixedCampaignInsightDto>(ins);

                    var manual = manualMap.TryGetValue(ins.CampaignId, out var m) ? m : 0;
                    dto.ManualFollowers = manual;

                    // Se fuerza manual el ResultadoPrincipal y ValorResultado
                    switch (ins.Objective)
                    {
                        case "LINK_CLICKS":
                            dto.ResultadoPrincipal = "Nuevos seguidores";
                            dto.ValorResultado     = manual;
                            break;

                        case "OUTCOME_SALES":
                            dto.ResultadoPrincipal = "Ventas";
                            dto.ValorResultado     = manual;
                            break;

                        default:
                            dto.ResultadoPrincipal = "Resultado manual";
                            dto.ValorResultado     = manual;
                            break;
                    }

                    // Recordatorio de demas objectives:
                    /* APP_INSTALLS, BRAND_AWARENESS, CONVERSIONS, EVENT_RESPONSES, LEAD_GENERATION, LINK_CLICKS,
                       LOCAL_AWARENESS, MESSAGES, OFFER_CLAIMS, OUTCOME_APP_PROMOTION, OUTCOME_AWARENESS, OUTCOME_ENGAGEMENT,
                       OUTCOME_LEADS, OUTCOME_SALES, OUTCOME_TRAFFIC, PAGE_LIKES, POST_ENGAGEMENT, PRODUCT_CATALOG_SALES, REACH,
                       STORE_VISITS, VIDEO_VIEWS */ 

                    return dto;
                })
                .ToList();

            return mixed;
        }
    }
}
