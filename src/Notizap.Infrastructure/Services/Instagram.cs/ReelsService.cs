using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class ReelsService : IReelsService
{
    private readonly HttpClient _httpClient;
    private readonly MetricoolSettings _settings;
    private readonly NotizapDbContext _context;

    public ReelsService(HttpClient httpClient, IOptions<MetricoolSettings> settings, NotizapDbContext context)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _context = context;
    }
    public async Task<List<ReelMetricDto>> GetInstagramReelsAsync(string accountName, DateTime from, DateTime to)
    {
        if (!_settings.BlogIds.TryGetValue(accountName.ToLower(), out var blogId))
            throw new ArgumentException("Cuenta desconocida", nameof(accountName));

        var start = from.ToString("yyyy-MM-ddTHH:mm:ss");
        var end = to.ToString("yyyy-MM-ddTHH:mm:ss");

        var url = $"https://app.metricool.com/api/v2/analytics/reels/instagram" +
                $"?from={start}&to={end}" +
                $"&userId={_settings.UserId}" +
                $"&blogId={blogId}" +
                $"&userToken={_settings.AccessToken}" +
                $"&timezone=America/Argentina/Buenos_Aires";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<ReelsApiResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;

        return result.Data;
    }
    public async Task<int> SyncInstagramReelsAsync(string accountName, DateTime from, DateTime to)
    {
        var reels = await GetInstagramReelsAsync(accountName, from, to);

        var existingIds = _context.InstagramReels
            .Where(r => r.Cuenta == accountName.ToLower())
            .Select(r => r.ReelId)
            .ToHashSet();

        var newReels = reels
            .Where(r => !existingIds.Contains(r.ReelId))
            .Select(r => new InstagramReel
            {
                ReelId = r.ReelId,
                Cuenta = accountName.ToLower(),
                FechaPublicacion = DateTime.SpecifyKind(
                    DateTime.Parse(r.PublishedAt.DateTime), DateTimeKind.Utc),
                Url = r.Url,
                ImageUrl = r.ImageUrl,
                Contenido = r.Content,
                Likes = r.Likes,
                Comentarios = r.Comments,
                Views = r.Views,
                Reach = r.Reach,
                Engagement = r.Engagement,
                Interacciones = (int)r.Interactions,
                VideoViews = r.VideoViews,
                Guardados = r.Saved,
                Compartidos = r.Shares,
                BusinessId = r.BusinessId
            }).ToList();

        if (newReels.Any())
        {
            _context.InstagramReels.AddRange(newReels);
            await _context.SaveChangesAsync();
        }

        return newReels.Count;
    }
    public async Task<List<InstagramReel>> GetTopReelsByViewsAsync(string accountName, DateTime from, DateTime to, int limit = 5)
    {
        from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        to = DateTime.SpecifyKind(to, DateTimeKind.Utc);

        return await _context.InstagramReels
            .Where(r => r.Cuenta == accountName.ToLower() && r.FechaPublicacion >= from && r.FechaPublicacion <= to)
            .OrderByDescending(r => r.Views)
            .Take(limit)
            .ToListAsync();
    }
    public async Task<List<InstagramReel>> GetTopReelsByLikesAsync(string accountName, DateTime from, DateTime to, int limit = 5)
    {
        from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        to = DateTime.SpecifyKind(to, DateTimeKind.Utc);

        return await _context.InstagramReels
            .Where(r => r.Cuenta == accountName.ToLower() && r.FechaPublicacion >= from && r.FechaPublicacion <= to)
            .OrderByDescending(r => r.Likes)
            .Take(limit)
            .ToListAsync();
    }
    public async Task<List<InstagramReel>> GetAllReelsAsync(string accountName, DateTime from, DateTime to)
    {
        from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        to = DateTime.SpecifyKind(to, DateTimeKind.Utc);

        return await _context.InstagramReels
            .Where(r => r.Cuenta == accountName.ToLower() && r.FechaPublicacion >= from && r.FechaPublicacion <= to)
            .OrderByDescending(r => r.FechaPublicacion)
            .ToListAsync();
    }
}
