using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

public class MailchimpService(HttpClient client, IOptions<MailchimpSettings> config) : IMailchimpService
{
    private readonly MailchimpSettings _mailchimpSettings = config.Value;
    public async Task<List<MailchimpCampaignDto>> GetAvailableCampaignsAsync()
    {
        client.BaseAddress = new Uri($"https://{_mailchimpSettings.ServerPrefix}.api.mailchimp.com/3.0/");
        var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"anystring:{_mailchimpSettings.ApiKey}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

        var response = await client.GetAsync("campaigns?sort_field=create_time&sort_dir=DESC&status=sent&count=10");
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
        client.BaseAddress = new Uri($"https://{_mailchimpSettings.ServerPrefix}.api.mailchimp.com/3.0/");
        var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"anystring:{_mailchimpSettings.ApiKey}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

        string id = campaignId ?? await GetLatestIdCampaignAsync();

        // Obtencion de detalles de la campaña
        var campaignResponse = await client.GetAsync($"campaigns/{id}");
        campaignResponse.EnsureSuccessStatusCode();
        var campaignJson = await campaignResponse.Content.ReadAsStringAsync();
        using var campaignDoc = JsonDocument.Parse(campaignJson);

        var title = campaignDoc.RootElement.GetProperty("settings").GetProperty("title").GetString();
        var sendTime = campaignDoc.RootElement.GetProperty("send_time").GetDateTime();
        var emailsSent = campaignDoc.RootElement.GetProperty("emails_sent").GetInt32();

        // Obtencion de estadisticas
        var reportResponse = await client.GetAsync($"reports/{id}");
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

    public async Task<MailchimpHighlightsDto> GetHighlightsAsync()
    {
        client.BaseAddress = new Uri($"https://{_mailchimpSettings.ServerPrefix}.api.mailchimp.com/3.0/");
        var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"anystring:{_mailchimpSettings.ApiKey}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

        var response = await client.GetAsync("campaigns?status=sent&sort_dir=DESC&count=10");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var campaigns = doc.RootElement.GetProperty("campaigns").EnumerateArray().ToList();

        string? bestOpen = null;
        string? bestClick = null;
        string? bestConv = null;
        decimal maxOpen = 0, maxClick = 0;
        int maxConv = 0;

        foreach (var c in campaigns)
        {
        var id = c.GetProperty("id").GetString();
        var subject = c.GetProperty("settings").GetProperty("subject_line").GetString();

        var reportResponse = await client.GetAsync($"reports/{id}");
        if (!reportResponse.IsSuccessStatusCode || subject is null) continue;

        var reportJson = await reportResponse.Content.ReadAsStringAsync();
        using var report = JsonDocument.Parse(reportJson);

        var open = report.RootElement.GetProperty("opens").GetProperty("open_rate").GetDecimal();
        var click = report.RootElement.GetProperty("clicks").GetProperty("click_rate").GetDecimal();
        var conv = report.RootElement.TryGetProperty("ecommerce", out var ecom) &&
                    ecom.TryGetProperty("total_orders", out var orders) ? orders.GetInt32() : 0;

        if (open > maxOpen) { maxOpen = open; bestOpen = subject; }
        if (click > maxClick) { maxClick = click; bestClick = subject; }
        if (conv > maxConv) { maxConv = conv; bestConv = subject; }
        }

        return new MailchimpHighlightsDto
        {
        BestOpenRateCampaign = bestOpen ?? "Sin datos",
        BestOpenRate = maxOpen,
        BestClickRateCampaign = bestClick ?? "Sin datos",
        BestClickRate = maxClick,
        BestConversionCampaign = bestConv ?? "Sin datos",
        BestConversions = maxConv
        };
    }
    private async Task<string> GetLatestIdCampaignAsync()
    {
        var response = await client.GetAsync("campaigns?sort_field=create_time&sort_dir=DESC&status=sent&count=1");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement.GetProperty("campaigns")[0].GetProperty("id").GetString()!;
    }
}