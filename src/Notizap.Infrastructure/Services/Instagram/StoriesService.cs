using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class StoriesService : IStoriesService
{
    private readonly HttpClient _httpClient;
    private readonly MetricoolSettings _settings;
    private readonly NotizapDbContext _context;

    public StoriesService(HttpClient httpClient, IOptions<MetricoolSettings> options, NotizapDbContext context)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _context = context;
    }

    public async Task<int> SyncInstagramStoriesAsync(string accountName, DateTime from, DateTime to)
    {
        if (!_settings.BlogIds.TryGetValue(accountName.ToLower(), out var blogId))
            throw new ArgumentException("Cuenta desconocida", nameof(accountName));

        var start = from.ToString("yyyy-MM-ddTHH:mm:ss");
        var end = to.ToString("yyyy-MM-ddTHH:mm:ss");

        var url = $"https://app.metricool.com/api/v2/analytics/stories/instagram" +
                  $"?from={start}&to={end}" +
                  $"&userId={_settings.UserId}" +
                  $"&blogId={blogId}" +
                  $"&userToken={_settings.AccessToken}" +
                  $"&timezone=America/Argentina/Buenos_Aires";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<StoriesApiResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;

        var existingIds = _context.InstagramStories
            .Where(s => s.Cuenta == accountName.ToLower())
            .Select(s => s.PostId)
            .ToHashSet();

        var historiasValidas = result.Data
            .Where(s => !string.IsNullOrWhiteSpace(s.PostId))
            .ToList();

        var nuevas = historiasValidas
            .Where(s => !existingIds.Contains(s.PostId))
            .Select(s => new InstagramStory
            {
                PostId = s.PostId!,
                Cuenta = accountName.ToLower(),
                FechaPublicacion = DateTime.SpecifyKind(DateTime.Parse(s.PublishedAt.DateTime), DateTimeKind.Utc),
                MediaUrl = s.MediaUrl,
                ThumbnailUrl = s.ThumbnailUrl,
                Permalink = s.Permalink,
                Content = s.Content,
                Impressions = s.Impressions,
                Reach = s.Reach,
                Replies = s.Replies,
                TapsForward = s.TapsForward,
                TapsBack = s.TapsBack,
                Exits = s.Exits,
                BusinessId = s.BusinessId
            }).ToList();

        if (nuevas.Any())
        {
            _context.InstagramStories.AddRange(nuevas);
            await _context.SaveChangesAsync();
        }

        return nuevas.Count;
    }

    public async Task<List<InstagramStory>> GetTopStoriesAsync(string accountName, DateTime from, DateTime to, string criterio, int limit = 10)
    {
        from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        to = DateTime.SpecifyKind(to, DateTimeKind.Utc);

        var query = _context.InstagramStories
            .Where(s => s.Cuenta == accountName.ToLower() && s.FechaPublicacion >= from && s.FechaPublicacion <= to);

        return criterio.ToLower() switch
        {
            "impressions" => await query.OrderByDescending(s => s.Impressions).Take(limit).ToListAsync(),
            "reach" => await query.OrderByDescending(s => s.Reach).Take(limit).ToListAsync(),
            "replies" => await query.OrderByDescending(s => s.Replies).Take(limit).ToListAsync(),
            "tapsforward" => await query.OrderByDescending(s => s.TapsForward).Take(limit).ToListAsync(),
            "exits" => await query.OrderByDescending(s => s.Exits).Take(limit).ToListAsync(),
            _ => throw new ArgumentException("Criterio inválido", nameof(criterio))
        };
    }
}
