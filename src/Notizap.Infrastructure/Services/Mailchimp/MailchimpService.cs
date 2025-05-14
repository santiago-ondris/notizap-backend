using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class MailchimpService : IMailchimpService
{
    private readonly HttpClient _client;
    private readonly MailchimpAccountSettings _config;

    public MailchimpService(HttpClient client, MailchimpAccountSettings config)
    {
        _client = client;
        _config = config;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint)
    {
        var request = new HttpRequestMessage(method, $"https://{_config.ServerPrefix}.api.mailchimp.com/3.0/{endpoint}");
        var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"anystring:{_config.ApiKey}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
        return request;
    }

    public async Task<List<MailchimpCampaignDto>> GetAvailableCampaignsAsync()
    {
        var request = CreateRequest(HttpMethod.Get, "campaigns?sort_field=create_time&sort_dir=DESC&status=sent&count=10");
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement.GetProperty("campaigns")
            .EnumerateArray()
            .Select(c => new MailchimpCampaignDto
            {
                Id = c.GetProperty("id").GetString()!,
                Title = c.GetProperty("settings").GetProperty("title").GetString()!,
                SendTime = c.GetProperty("send_time").GetDateTime()
            })
            .ToList();
    }

    public async Task<MailchimpStatsDto> GetCampaignStatsAsync(string? campaignId = null)
    {
        string id = campaignId ?? await GetLatestIdCampaignAsync();

        var campaignRequest = CreateRequest(HttpMethod.Get, $"campaigns/{id}");
        var campaignResponse = await _client.SendAsync(campaignRequest);
        campaignResponse.EnsureSuccessStatusCode();
        var campaignJson = await campaignResponse.Content.ReadAsStringAsync();
        using var campaignDoc = JsonDocument.Parse(campaignJson);

        var title = campaignDoc.RootElement.GetProperty("settings").GetProperty("title").GetString();
        var sendTime = campaignDoc.RootElement.GetProperty("send_time").GetDateTime();
        var emailsSent = campaignDoc.RootElement.GetProperty("emails_sent").GetInt32();

        var reportRequest = CreateRequest(HttpMethod.Get, $"reports/{id}");
        var reportResponse = await _client.SendAsync(reportRequest);
        reportResponse.EnsureSuccessStatusCode();
        var reportJson = await reportResponse.Content.ReadAsStringAsync();
        using var reportDoc = JsonDocument.Parse(reportJson);

        return new MailchimpStatsDto
        {
            CampaignTitle = title!,
            SendTime = sendTime,
            EmailsSent = emailsSent,
            OpenRate = reportDoc.RootElement.GetProperty("opens").GetProperty("open_rate").GetDecimal(),
            ClickRate = reportDoc.RootElement.GetProperty("clicks").GetProperty("click_rate").GetDecimal(),
            Conversions = reportDoc.RootElement.TryGetProperty("ecommerce", out var ecommerce) &&
                          ecommerce.TryGetProperty("total_orders", out var orders)
                ? orders.GetInt32()
                : 0
        };
    }

    private async Task<string> GetLatestIdCampaignAsync()
    {
        var request = CreateRequest(HttpMethod.Get, "campaigns?sort_field=create_time&sort_dir=DESC&status=sent&count=1");
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement.GetProperty("campaigns")[0].GetProperty("id").GetString()!;
    }
}
