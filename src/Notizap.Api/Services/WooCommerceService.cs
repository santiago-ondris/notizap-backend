using System.Text.Json;
using Microsoft.Extensions.Options;

namespace NotiZap.Dashboard.API.Services
{
  public class WooCommerceService : IWooCommerceService
  {
    private readonly HttpClient _client;
    private readonly WooCommerceSettings _settings;

    public WooCommerceService(HttpClient client, IOptions<WooCommerceSettings> cfg)
    {
      _client = client;
      _settings = cfg.Value;
      _client.BaseAddress = new Uri(_settings.ApiUrl);
    }

    public async Task<SalesStatsDto> GetMonthlyStatsSimpleAsync(int year, int month)
    {
        // 1. Rango de fechas
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1).AddSeconds(-1);

        var allOrders = await FetchAllOrdersAsync(start, end);

        // 3. Agrupar por día y parsear revenue
        var byDay = allOrders
          .Select(o =>
          {
              var date = DateTime.Parse(o.GetProperty("date_created").GetString()!);
              var units = o.GetProperty("line_items")
                            .EnumerateArray()
                            .Sum(li => li.GetProperty("quantity").GetInt32());

              var totalStr = o.GetProperty("total").GetString()!;
              var revenue = decimal.Parse(
                totalStr,
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture
              );

              return new { Date = date.Date, Units = units, Revenue = revenue };
          })
          .GroupBy(x => x.Date)
          .ToDictionary(
              g => g.Key,
              g => new { Units = g.Sum(x => x.Units), Revenue = g.Sum(x => x.Revenue) }
          );

        // 4. Construir DTO
        var stats = new SalesStatsDto
        {
            Year = year,
            Month = month,
            DailyGoal = 0 // Omito meta, ver en un futuro
        };

        int daysInMonth = DateTime.DaysInMonth(year, month);
        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year, month, day);
            if (byDay.TryGetValue(date, out var v))
            {
                stats.DailySales.Add(new DailySalesDto
                {
                    Date = date,
                    UnitsSold = v.Units,
                    Revenue = v.Revenue
                });
                stats.UnitsSold += v.Units;
                stats.TotalRevenue += v.Revenue;
            }
            else
            {
                stats.DailySales.Add(new DailySalesDto
                {
                    Date = date,
                    UnitsSold = 0,
                    Revenue = 0
                });
            }
        }

        return stats;
    }

    private async Task<List<JsonElement>> FetchAllOrdersAsync(DateTime start, DateTime end)
    {
        const int perPage = 100;
        // 1) Petición inicial para página 1
        var urlBase = $"orders?consumer_key={_settings.ConsumerKey}" +
                    $"&consumer_secret={_settings.ConsumerSecret}" +
                    $"&status=completed" +
                    $"&after={start:O}&before={end:O}" +
                    $"&per_page={perPage}&page=1";
        var firstResp = await _client.GetAsync(urlBase);
        firstResp.EnsureSuccessStatusCode();

        // 2) Lee el header que dice cuántas páginas hay
        //    WooCommerce expone "X-WP-TotalPages"
        if (!firstResp.Headers.TryGetValues("X-WP-TotalPages", out var hdr))
            throw new InvalidOperationException("No se encontró X-WP-TotalPages");
        int totalPages = int.Parse(hdr.First());

        // 3) Deserializa la página 1
        var allOrders = JsonSerializer.Deserialize<List<JsonElement>>(
            await firstResp.Content.ReadAsStringAsync()
        )!;

        // 4) Prepara tareas para las páginas restantes (2…totalPages)
        var tasks = Enumerable.Range(2, totalPages - 1)
            .Select(async page =>
            {
                var resp = await _client.GetAsync(
                    $"orders?consumer_key={_settings.ConsumerKey}" +
                    $"&consumer_secret={_settings.ConsumerSecret}" +
                    $"&status=completed" +
                    $"&after={start:O}&before={end:O}" +
                    $"&per_page={perPage}&page={page}"
                );
                resp.EnsureSuccessStatusCode();
                return JsonSerializer.Deserialize<List<JsonElement>>(
                    await resp.Content.ReadAsStringAsync()
                )!;
            })
            .ToList();

        // 5) Espera a que terminen todas las peticiones
        var pages = await Task.WhenAll(tasks);

        // 6) Aplana los resultados y devuelve
        return allOrders
            .Concat(pages.SelectMany(p => p))
            .ToList();
    }

    public async Task<SalesStatsDto> GetStatsByRangeAsync(DateTime from, DateTime to)
    {
        // 1) Trae todos los pedidos entre 'from' y 'to'
        var allOrders = await FetchAllOrdersAsync(from, to);

        // 2) Construye el DTO según ese rango
        return BuildStatsByRange(allOrders, from, to);
    }

    // Extrae la lógica de agrupación en un helper:
    private SalesStatsDto BuildStatsByRange(
        List<JsonElement> allOrders,
        DateTime from,
        DateTime to)
    {
        // Calcula días totales en el rango
        int totalDays = (to.Date - from.Date).Days + 1;

        var stats = new SalesStatsDto
        {
            Year         = from.Year,              // opcional, solo para información
            Month        = from.Month,             // idem
            DailyGoal    = 0,                      // sin meta aquí
            UnitsSold    = 0,
            TotalRevenue = 0m,
            DailySales   = new List<DailySalesDto>()
        };

        // Inicializa un diccionario día→suma
        var dict = allOrders
        .Select(o => new {
            Date    = DateTime.Parse(o.GetProperty("date_created").GetString()!).Date,
            Units   = o.GetProperty("line_items")
                            .EnumerateArray()
                            .Sum(li => li.GetProperty("quantity").GetInt32()),
            Revenue = decimal.Parse(
                            o.GetProperty("total").GetString()!,
                            System.Globalization.NumberStyles.Number,
                            System.Globalization.CultureInfo.InvariantCulture)
        })
        .GroupBy(x => x.Date)
        .ToDictionary(g => g.Key, g => new {
            Units   = g.Sum(x => x.Units),
            Revenue = g.Sum(x => x.Revenue)
        });

        // Rellena cada día del rango
        for (int i = 0; i < totalDays; i++)
        {
            var day = from.Date.AddDays(i);
            if (dict.TryGetValue(day, out var v))
            {
                stats.DailySales.Add(new DailySalesDto {
                    Date      = day,
                    UnitsSold = v.Units,
                    Revenue   = v.Revenue
                });
                stats.UnitsSold    += v.Units;
                stats.TotalRevenue += v.Revenue;
            }
            else
            {
                stats.DailySales.Add(new DailySalesDto {
                    Date      = day,
                    UnitsSold = 0,
                    Revenue   = 0
                });
            }
        }

        return stats;
    }
  
    public async Task<List<ProductStatsDto>> GetTopProductsAsync(DateTime from, DateTime to, int topN = 5)
        {
            // 1) Trae todos los pedidos en el rango
            var allOrders = await FetchAllOrdersAsync(from, to);

            // 2) Agrega unidades por nombre base de producto
            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var order in allOrders)
            {
                foreach (var li in order.GetProperty("line_items").EnumerateArray())
                {
                    // Nombre completo incluye talla: "Bota XYZ - 36"
                    var rawName = li.GetProperty("name").GetString()!;
                    
                    // Tomar solo la parte antes del primer " - "
                    var baseName = rawName.Contains(" - ")
                        ? rawName.Split(" - ", 2)[0]
                        : rawName;

                    var qty = li.GetProperty("quantity").GetInt32();

                    if (dict.ContainsKey(baseName))
                        dict[baseName] += qty;
                    else
                        dict[baseName] = qty;
                }
            }

            // 3) Ordena y toma el topN
            var topList = dict
                .OrderByDescending(kvp => kvp.Value)
                .Take(topN)
                .Select(kvp => new ProductStatsDto
                {
                    ProductName = kvp.Key,
                    UnitsSold   = kvp.Value
                })
                .ToList();

            return topList;
        }
  }
}

