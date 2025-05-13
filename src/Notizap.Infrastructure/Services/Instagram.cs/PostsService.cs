using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class PostsService : IPostsService
{
    private readonly HttpClient _httpClient;
    private readonly MetricoolSettings _settings;
    private readonly NotizapDbContext _context;

    public PostsService(HttpClient httpClient, IOptions<MetricoolSettings> options, NotizapDbContext context)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _context = context;
    }

    public async Task<int> SyncInstagramPostsAsync(string accountName, DateTime from, DateTime to)
    {
        if (!_settings.BlogIds.TryGetValue(accountName.ToLower(), out var blogId))
            throw new ArgumentException("Cuenta desconocida", nameof(accountName));

        var start = from.ToString("yyyy-MM-ddTHH:mm:ss");
        var end = to.ToString("yyyy-MM-ddTHH:mm:ss");

        var url = $"https://app.metricool.com/api/v2/analytics/posts/instagram" +
                  $"?from={start}&to={end}" +
                  $"&userId={_settings.UserId}" +
                  $"&blogId={blogId}" +
                  $"&userToken={_settings.AccessToken}" +
                  $"&timezone=America/Argentina/Buenos_Aires";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PostsApiResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;

        var existingIds = _context.InstagramPosts
            .Where(p => p.Cuenta == accountName.ToLower())
            .Select(p => p.PostId)
            .ToHashSet();

        var nuevos = result.Data
            .Where(p => !existingIds.Contains(p.PostId))
            .Select(p => new InstagramPost
            {
                PostId = p.PostId,
                Cuenta = accountName.ToLower(),
                FechaPublicacion = DateTime.SpecifyKind(DateTime.Parse(p.PublishedAt.DateTime), DateTimeKind.Utc),
                Url = p.Url,
                ImageUrl = p.ImageUrl,
                Content = string.IsNullOrWhiteSpace(p.Content) ? null : p.Content,
                Likes = p.Likes,
                Comments = p.Comments,
                Shares = p.Shares,
                Interactions = p.Interactions,
                Engagement = p.Engagement,
                Impressions = p.Impressions,
                Reach = p.Reach,
                Saved = p.Saved,
                VideoViews = p.VideoViews,
                Clicks = p.Clicks,
                BusinessId = p.BusinessId
            }).ToList();

        if (nuevos.Any())
        {
            _context.InstagramPosts.AddRange(nuevos);
            await _context.SaveChangesAsync();
        }

        return nuevos.Count;
    }

    public async Task<List<InstagramPost>> GetTopPostsAsync(string accountName, DateTime from, DateTime to, string ordenarPor, int limit = 5)
    {
        from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        to = DateTime.SpecifyKind(to, DateTimeKind.Utc);

        var query = _context.InstagramPosts
            .Where(p => p.Cuenta == accountName.ToLower() && p.FechaPublicacion >= from && p.FechaPublicacion <= to);

        return ordenarPor.ToLower() switch
        {
            "likes" => await query.OrderByDescending(p => p.Likes).Take(limit).ToListAsync(),
            "comments" => await query.OrderByDescending(p => p.Comments).Take(limit).ToListAsync(),
            "interactions" => await query.OrderByDescending(p => p.Interactions).Take(limit).ToListAsync(),
            "engagement" => await query.OrderByDescending(p => p.Engagement).Take(limit).ToListAsync(),
            "impressions" => await query.OrderByDescending(p => p.Impressions).Take(limit).ToListAsync(),
            _ => throw new ArgumentException("Criterio de orden inválido", nameof(ordenarPor))
        };
    }
}
