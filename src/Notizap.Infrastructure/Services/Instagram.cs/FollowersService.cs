using System.Text.Json;
using Microsoft.Extensions.Options;

public class FollowersService : IFollowersService
{
    private readonly HttpClient _httpClient;
    private readonly MetricoolSettings _settings;

    public FollowersService(HttpClient httpClient, IOptions<MetricoolSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<List<FollowerDayData>> GetFollowersMetricsAsync(string accountName, DateTime from, DateTime to)
    {
        if (!_settings.BlogIds.TryGetValue(accountName.ToLower(), out var blogId))
            throw new ArgumentException("Cuenta desconocida", nameof(accountName));

        var start = from.ToString("yyyyMMdd");
        var end = to.ToString("yyyyMMdd");

        var url = $"https://app.metricool.com/api/stats/timeline/igFollowers" +
                $"?start={start}&end={end}" +
                $"&userId={_settings.UserId}" +
                $"&blogId={blogId}" +
                $"&userToken={_settings.AccessToken}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var rawArray = JsonSerializer.Deserialize<List<List<string>>>(json)!;

        var result = rawArray.Select(item => new FollowerDayData
        {
            Date = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(item[0])).DateTime,
            Value = int.Parse(item[1].Split('.')[0])
        }).ToList();

        return result;
    }
}    