using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace NotiZap.Dashboard.API.Services
{
  public class WooCommerceService : IWooCommerceService
  {
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly NotizapDbContext _context;

    public WooCommerceService(IHttpClientFactory httpClientFactory, IConfiguration configuration, NotizapDbContext context)
    {
      _httpClientFactory = httpClientFactory;
      _configuration = configuration;
      _context = context;
    }

    private HttpClient CreateClient(WooCommerceStore store)
    {
      var section = _configuration.GetSection($"WooCommerce:{store}");
      var apiUrl = section["ApiUrl"]!;
      var consumerKey = section["ConsumerKey"];
      var consumerSecret = section["ConsumerSecret"];

      var client = _httpClientFactory.CreateClient();
      client.BaseAddress = new Uri(apiUrl);
      client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
          "Basic",
          Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{consumerKey}:{consumerSecret}"))
      );

      return client;
    }

    public async Task<SalesStatsDto> GetMonthlyStatsSimpleAsync(int year, int month, WooCommerceStore store)
    {
      var start = new DateTime(year, month, 1);
      var end = start.AddMonths(1).AddSeconds(-1);

      var client = CreateClient(store);
      var allOrders = await FetchAllOrdersAsync(client, start, end);

      var byDay = allOrders
        .Select(o =>
        {
          var date = DateTime.Parse(o.GetProperty("date_created").GetString()!);
          var units = o.GetProperty("line_items").EnumerateArray().Sum(li => li.GetProperty("quantity").GetInt32());
          var totalStr = o.GetProperty("total").GetString()!;
          var revenue = decimal.Parse(totalStr, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture);
          return new { Date = date.Date, Units = units, Revenue = revenue };
        })
        .GroupBy(x => x.Date)
        .ToDictionary(g => g.Key, g => new { Units = g.Sum(x => x.Units), Revenue = g.Sum(x => x.Revenue) });

      var stats = new SalesStatsDto
      {
        Year = year,
        Month = month,
        DailyGoal = 0
      };

      int daysInMonth = DateTime.DaysInMonth(year, month);
      for (int day = 1; day <= daysInMonth; day++)
      {
        var date = new DateTime(year, month, day);
        if (byDay.TryGetValue(date, out var v))
        {
          stats.DailySales.Add(new DailySalesDto { Date = date, UnitsSold = v.Units, Revenue = v.Revenue });
          stats.UnitsSold += v.Units;
          stats.TotalRevenue += v.Revenue;
        }
        else
        {
          stats.DailySales.Add(new DailySalesDto { Date = date, UnitsSold = 0, Revenue = 0 });
        }
      }

      return stats;
    }

    private async Task<List<JsonElement>> FetchAllOrdersAsync(HttpClient client, DateTime start, DateTime end)
    {
      const int perPage = 100;
      var urlBase = $"orders?status=completed&after={start:O}&before={end:O}&per_page={perPage}&page=1";

      var firstResp = await client.GetAsync(urlBase);
      firstResp.EnsureSuccessStatusCode();

      if (!firstResp.Headers.TryGetValues("X-WP-TotalPages", out var hdr))
        throw new InvalidOperationException("No se encontró X-WP-TotalPages");

      int totalPages = int.Parse(hdr.First());

      var allOrders = JsonSerializer.Deserialize<List<JsonElement>>(await firstResp.Content.ReadAsStringAsync())!;

      var tasks = Enumerable.Range(2, totalPages - 1)
          .Select(async page =>
          {
            var resp = await client.GetAsync($"{urlBase}&page={page}");
            resp.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<List<JsonElement>>(await resp.Content.ReadAsStringAsync())!;
          });

      var pages = await Task.WhenAll(tasks);
      return allOrders.Concat(pages.SelectMany(p => p)).ToList();
    }

    public async Task<SalesStatsDto> GetStatsByRangeAsync(DateTime from, DateTime to, WooCommerceStore store)
    {
      var client = CreateClient(store);
      var allOrders = await FetchAllOrdersAsync(client, from, to);
      return BuildStatsByRange(allOrders, from, to);
    }

    private SalesStatsDto BuildStatsByRange(List<JsonElement> allOrders, DateTime from, DateTime to)
    {
      int totalDays = (to.Date - from.Date).Days + 1;

      var stats = new SalesStatsDto
      {
        Year = from.Year,
        Month = from.Month,
        DailyGoal = 0,
        UnitsSold = 0,
        TotalRevenue = 0m,
        DailySales = new List<DailySalesDto>()
      };

      var dict = allOrders
        .Select(o => new
        {
          Date = DateTime.Parse(o.GetProperty("date_created").GetString()!).Date,
          Units = o.GetProperty("line_items").EnumerateArray().Sum(li => li.GetProperty("quantity").GetInt32()),
          Revenue = decimal.Parse(o.GetProperty("total").GetString()!, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture)
        })
        .GroupBy(x => x.Date)
        .ToDictionary(g => g.Key, g => new { Units = g.Sum(x => x.Units), Revenue = g.Sum(x => x.Revenue) });

      for (int i = 0; i < totalDays; i++)
      {
        var day = from.Date.AddDays(i);
        if (dict.TryGetValue(day, out var v))
        {
          stats.DailySales.Add(new DailySalesDto { Date = day, UnitsSold = v.Units, Revenue = v.Revenue });
          stats.UnitsSold += v.Units;
          stats.TotalRevenue += v.Revenue;
        }
        else
        {
          stats.DailySales.Add(new DailySalesDto { Date = day, UnitsSold = 0, Revenue = 0 });
        }
      }

      return stats;
    }

    public async Task<List<ProductStatsDto>> GetTopProductsAsync(DateTime from, DateTime to, WooCommerceStore store, int topN = 5)
    {
      var client = CreateClient(store);
      var allOrders = await FetchAllOrdersAsync(client, from, to);

      var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

      foreach (var order in allOrders)
      {
        foreach (var li in order.GetProperty("line_items").EnumerateArray())
        {
          var rawName = li.GetProperty("name").GetString()!;
          var baseName = rawName.Contains(" - ") ? rawName.Split(" - ", 2)[0] : rawName;
          var qty = li.GetProperty("quantity").GetInt32();

          if (dict.ContainsKey(baseName))
            dict[baseName] += qty;
          else
            dict[baseName] = qty;
        }
      }

      return dict
        .OrderByDescending(kvp => kvp.Value)
        .Take(topN)
        .Select(kvp => new ProductStatsDto { ProductName = kvp.Key, UnitsSold = kvp.Value })
        .ToList();
    }
    public async Task<WooCommerceMonthlyReport> SaveMonthlyReportAsync(SaveWooMonthlyReportDto dto)
    {
        var existing = await _context.WooCommerceMonthlyReports
            .FirstOrDefaultAsync(r => r.Year == dto.Year && r.Month == dto.Month && r.Store == dto.Store);

        if (existing != null)
        {
            existing.UnitsSold = dto.UnitsSold;
            existing.Revenue = dto.Revenue;
            existing.DailySales = dto.DailySales.Select(d => new WooDailySale
                {
                    Date = DateTime.SpecifyKind(d.Date, DateTimeKind.Utc),
                    UnitsSold = d.UnitsSold,
                    Revenue = d.Revenue
                }).ToList();
        }
        else
        {
            existing = new WooCommerceMonthlyReport
            {
                Year = dto.Year,
                Month = dto.Month,
                Store = dto.Store,
                UnitsSold = dto.UnitsSold,
                Revenue = dto.Revenue,
                DailySales = dto.DailySales.Select(d => new WooDailySale
                {
                    Date = DateTime.SpecifyKind(d.Date, DateTimeKind.Utc),
                    UnitsSold = d.UnitsSold,
                    Revenue = d.Revenue
                }).ToList()
            };

            _context.WooCommerceMonthlyReports.Add(existing);
        }

        await _context.SaveChangesAsync();
        return existing;
    }
    public async Task<WooCommerceMonthlyReport?> GetSavedMonthlyReportAsync(int year, int month, WooCommerceStore store)
    {
        return await _context.WooCommerceMonthlyReports
            .Include(r => r.DailySales)
            .FirstOrDefaultAsync(r => r.Year == year && r.Month == month && r.Store == store);
    }
  }
}
