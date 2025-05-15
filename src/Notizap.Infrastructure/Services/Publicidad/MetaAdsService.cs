using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Notizap.Infrastructure.Services.Publicidad
{
    public class MetaAdsService : IMetaAdsService
    {
        private readonly HttpClient _httpClient;
        private readonly MetaAdsSettings _settings;

        public MetaAdsService(HttpClient httpClient, IOptions<MetaAdsSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task<List<MetaCampaignInsightDto>> GetCampaignInsightsAsync(string adAccountId, DateTime from, DateTime to)
        {
            var result = new List<MetaCampaignInsightDto>();
            string baseUrl = $"https://graph.facebook.com/v19.0/{adAccountId}/insights" +
                             $"?fields=campaign_id,campaign_name,objective,spend,clicks,impressions,ctr,reach,date_start,date_stop,actions" +
                             $"&level=campaign" +
                             $"&time_range[since]={from:yyyy-MM-dd}" +
                             $"&time_range[until]={to:yyyy-MM-dd}" +
                             $"&access_token={_settings.Token}";

            string? nextUrl = baseUrl;

            while (!string.IsNullOrWhiteSpace(nextUrl))
            {
                var response = await _httpClient.GetAsync(nextUrl);
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Meta API error: {response.StatusCode}");

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var jsonDoc = await JsonDocument.ParseAsync(stream);
                var root = jsonDoc.RootElement;

                if (!root.TryGetProperty("data", out var dataElement) || dataElement.ValueKind != JsonValueKind.Array)
                    break;

                foreach (var item in dataElement.EnumerateArray())
                {
                    var campaignId   = GetString(item, "campaign_id");
                    var campaignName = GetString(item, "campaign_name");
                    var objective    = GetString(item, "objective");

                    var spend       = GetDecimal(item, "spend");
                    var clicks      = GetInt(item, "clicks");
                    var impressions = GetInt(item, "impressions");
                    var ctr         = GetDecimal(item, "ctr");
                    var reach       = GetInt(item, "reach");

                    var startDate = GetDate(item, "date_start");
                    var endDate   = GetDate(item, "date_stop");

                    JsonElement? actions = null;
                    if (item.TryGetProperty("actions", out var actProp) && actProp.ValueKind == JsonValueKind.Array)
                        actions = actProp;

                    var (descripcion, valor) = InterpretarResultado(objective, actions);

                    result.Add(new MetaCampaignInsightDto
                    {
                        CampaignId        = campaignId,
                        CampaignName      = campaignName,
                        Objective         = objective,
                        Spend             = spend,
                        Clicks            = clicks,
                        Impressions       = impressions,
                        Ctr               = ctr,
                        Reach             = reach,
                        Start             = startDate,
                        End               = endDate,
                        ResultadoPrincipal = descripcion,
                        ValorResultado    = valor
                    });
                }

                // Paginación
                if (root.TryGetProperty("paging", out var paging) &&
                    paging.TryGetProperty("next", out var nextProp) &&
                    !string.IsNullOrWhiteSpace(nextProp.GetString()))
                {
                    nextUrl = nextProp.GetString();
                }
                else
                {
                    nextUrl = null;
                }
            }

            return result;
        }

        private static string GetString(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) &&
                   prop.ValueKind == JsonValueKind.String
                   ? prop.GetString()!
                   : string.Empty;
        }

        private static decimal GetDecimal(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var prop) &&
                (prop.ValueKind == JsonValueKind.String || prop.ValueKind == JsonValueKind.Number) &&
                decimal.TryParse(prop.GetString(), out var value))
            {
                return value;
            }
            return 0m;
        }

        private static int GetInt(JsonElement element, string propertyName)
        {
            return (int)GetDecimal(element, propertyName);
        }

        private static DateTime GetDate(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var prop) &&
                prop.ValueKind == JsonValueKind.String &&
                DateTime.TryParse(prop.GetString(), out var date))
            {
                return date;
            }
            return DateTime.MinValue;
        }

        private (string descripcion, int valor) InterpretarResultado(string objective, JsonElement? actions)
        {
            if (actions == null || actions.Value.ValueKind != JsonValueKind.Array || actions.Value.GetArrayLength() == 0)
                return ("Sin datos", 0);

            foreach (var action in actions.Value.EnumerateArray())
            {
                if (!action.TryGetProperty("action_type", out var typeProp) ||
                    !action.TryGetProperty("value", out var valueProp))
                    continue;

                var type = typeProp.GetString() ?? string.Empty;
                if (!int.TryParse(valueProp.GetString(), out var value))
                    continue;

                return objective switch
                {
                    "OUTCOME_SALES"  when type == "purchase" => ("Ventas", value),
                    "LINK_CLICKS"    when type == "link_click" => ("Clics en el enlace", value),
                    "PROFILE_VISITS" when type == "profile_visits" => ("Visitas al perfil", value),
                    "FOLLOWERS"      when type == "like" => ("Seguidores", value),
                    "PAGE_LIKES"     when type == "like" => ("Seguidores", value),
                    _ => ("Sin datos", 0)
                };
            }

            // Fallback
            foreach (var action in actions.Value.EnumerateArray())
            {
                if (action.TryGetProperty("action_type", out var typeProp) &&
                    action.TryGetProperty("value", out var valueProp) &&
                    int.TryParse(valueProp.GetString(), out var val))
                {
                    return ($"Acción: {typeProp.GetString()}", val);
                }
            }

            return ("Sin resultado principal", 0);
        }
    }
}
