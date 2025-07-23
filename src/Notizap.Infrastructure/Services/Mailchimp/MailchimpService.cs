using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

public class MailchimpService : IMailchimpService
{
    private readonly HttpClient _client;
    private readonly MailchimpAccountSettings _config;
    private readonly ILogger<MailchimpService> _logger;

    public MailchimpService(HttpClient client, MailchimpAccountSettings config, ILogger<MailchimpService> logger)
    {
        _client = client;
        _config = config;
        _logger = logger;
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
        var endpoint = "campaigns?sort_field=create_time&sort_dir=DESC&status=sent&count=10";
        _logger.LogInformation("🌐 Llamando a API de Mailchimp para obtener campañas disponibles. Endpoint: {Endpoint}", endpoint);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var request = CreateRequest(HttpMethod.Get, endpoint);
            var response = await _client.SendAsync(request);
            
            stopwatch.Stop();
            var responseTime = stopwatch.ElapsedMilliseconds;

            // Log de performance - warning si es lenta
            if (responseTime > 3000)
            {
                _logger.LogWarning("⚠️ Respuesta lenta de API Mailchimp: {ResponseTime}ms (umbral: 3000ms)", responseTime);
            }
            else
            {
                _logger.LogInformation("⚡ API Mailchimp respondió en {ResponseTime}ms", responseTime);
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var campaigns = doc.RootElement.GetProperty("campaigns")
                .EnumerateArray()
                .Select(c => new MailchimpCampaignDto
                {
                    Id = c.GetProperty("id").GetString()!,
                    Title = c.GetProperty("settings").GetProperty("title").GetString()!,
                    SendTime = c.GetProperty("send_time").GetDateTime()
                })
                .ToList();

            _logger.LogInformation("✅ Se obtuvieron {CantidadCampanas} campañas desde API de Mailchimp en {ResponseTime}ms", 
                campaigns.Count, responseTime);

            // Log adicional con detalles de las campañas obtenidas
            if (campaigns.Any())
            {
                var campanaMasReciente = campaigns.First();
                _logger.LogInformation("📧 Campaña más reciente: '{Title}' enviada el {SendTime:yyyy-MM-dd HH:mm}", 
                    campanaMasReciente.Title, campanaMasReciente.SendTime);
            }

            return campaigns;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "🚫 Error de conectividad con API Mailchimp después de {ResponseTime}ms. Endpoint: {Endpoint}", 
                stopwatch.ElapsedMilliseconds, endpoint);
            throw new Exception($"Error de conectividad con Mailchimp: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "📄 Error al parsear respuesta JSON de API Mailchimp después de {ResponseTime}ms", 
                stopwatch.ElapsedMilliseconds);
            throw new Exception($"Error al procesar respuesta de Mailchimp: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "💥 Error inesperado con API Mailchimp después de {ResponseTime}ms", 
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<MailchimpStatsDto> GetCampaignStatsAsync(string? campaignId = null)
    {
        string id = campaignId ?? await GetLatestIdCampaignAsync();
        
        _logger.LogInformation("📊 Obteniendo estadísticas detalladas para campaña {CampaignId} desde API Mailchimp", id);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Obtener información básica de la campaña
            var campaignEndpoint = $"campaigns/{id}";
            _logger.LogDebug("🔍 Llamando a endpoint de campaña: {Endpoint}", campaignEndpoint);

            var campaignRequest = CreateRequest(HttpMethod.Get, campaignEndpoint);
            var campaignResponse = await _client.SendAsync(campaignRequest);
            campaignResponse.EnsureSuccessStatusCode();
            
            var campaignJson = await campaignResponse.Content.ReadAsStringAsync();
            using var campaignDoc = JsonDocument.Parse(campaignJson);

            var title = campaignDoc.RootElement.GetProperty("settings").GetProperty("title").GetString();
            var sendTime = campaignDoc.RootElement.GetProperty("send_time").GetDateTime();
            var emailsSent = campaignDoc.RootElement.GetProperty("emails_sent").GetInt32();

            _logger.LogDebug("📧 Datos básicos obtenidos - Título: '{Title}', Emails enviados: {EmailsSent}", title, emailsSent);

            // Obtener reporte de métricas
            var reportEndpoint = $"reports/{id}";
            _logger.LogDebug("📈 Llamando a endpoint de reporte: {Endpoint}", reportEndpoint);

            var reportRequest = CreateRequest(HttpMethod.Get, reportEndpoint);
            var reportResponse = await _client.SendAsync(reportRequest);
            reportResponse.EnsureSuccessStatusCode();
            
            var reportJson = await reportResponse.Content.ReadAsStringAsync();
            using var reportDoc = JsonDocument.Parse(reportJson);

            var openRate = reportDoc.RootElement.GetProperty("opens").GetProperty("open_rate").GetDecimal();
            var clickRate = reportDoc.RootElement.GetProperty("clicks").GetProperty("click_rate").GetDecimal();
            var conversions = reportDoc.RootElement.TryGetProperty("ecommerce", out var ecommerce) &&
                              ecommerce.TryGetProperty("total_orders", out var orders)
                ? orders.GetInt32()
                : 0;

            stopwatch.Stop();
            var responseTime = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation("✅ Estadísticas obtenidas para campaña {CampaignId} en {ResponseTime}ms - OpenRate: {OpenRate:F2}%, ClickRate: {ClickRate:F2}%, Conversiones: {Conversions}", 
                id, responseTime, openRate, clickRate, conversions);

            // Log de warning si las métricas son muy bajas (podría indicar problema)
            if (openRate < 5)
            {
                _logger.LogWarning("⚠️ Open rate muy bajo ({OpenRate:F2}%) para campaña {CampaignId} - '{Title}'", 
                    openRate, id, title);
            }

            return new MailchimpStatsDto
            {
                CampaignTitle = title!,
                SendTime = sendTime,
                EmailsSent = emailsSent,
                OpenRate = openRate,
                ClickRate = clickRate,
                Conversions = conversions
            };
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "🚫 Error de conectividad al obtener estadísticas de campaña {CampaignId} después de {ResponseTime}ms", 
                id, stopwatch.ElapsedMilliseconds);
            throw new Exception($"Error de conectividad con Mailchimp al obtener estadísticas: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "📄 Error al parsear estadísticas de campaña {CampaignId} después de {ResponseTime}ms", 
                id, stopwatch.ElapsedMilliseconds);
            throw new Exception($"Error al procesar estadísticas de Mailchimp: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "💥 Error inesperado al obtener estadísticas de campaña {CampaignId} después de {ResponseTime}ms", 
                id, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private async Task<string> GetLatestIdCampaignAsync()
    {
        var endpoint = "campaigns?sort_field=create_time&sort_dir=DESC&status=sent&count=1";
        _logger.LogDebug("🔍 Obteniendo ID de campaña más reciente. Endpoint: {Endpoint}", endpoint);

        try
        {
            var request = CreateRequest(HttpMethod.Get, endpoint);
            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var latestId = doc.RootElement.GetProperty("campaigns")[0].GetProperty("id").GetString()!;
            
            _logger.LogDebug("🎯 ID de campaña más reciente obtenido: {LatestCampaignId}", latestId);
            
            return latestId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener ID de campaña más reciente");
            throw new Exception($"Error al obtener campaña más reciente: {ex.Message}", ex);
        }
    }
}