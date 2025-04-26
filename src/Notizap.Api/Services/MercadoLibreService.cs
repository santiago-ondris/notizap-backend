using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Notizap.Services.MercadoLibre
{
    public class MercadoLibreService : IMercadoLibreService
    {
        private readonly HttpClient _httpClient;
        private readonly MercadoLibreSettings _settings;

        public MercadoLibreService(HttpClient httpClient, IOptions<MercadoLibreSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<SalesStatsDto> GetStatsByRangeAsync(DateTime from, DateTime to)
        {
            var orders = await FetchOrdersAsync(from, to);

            var groupedDailySales = new Dictionary<string, DailySalesDto>();

            int totalUnitsSold = 0;
            decimal totalRevenue = 0;

            foreach (var order in orders)
            {
                string dateKey = order.date_created.ToString("yyyy-MM-dd");

                if (!groupedDailySales.ContainsKey(dateKey))
                {
                    groupedDailySales[dateKey] = new DailySalesDto
                    {
                        Date = DateTime.Parse(dateKey),
                        UnitsSold = 0,
                        Revenue = 0
                    };
                }

                foreach (var item in order.order_items)
                {
                    int quantity = (int)item.quantity;
                    decimal price = (decimal)item.unit_price;

                    groupedDailySales[dateKey].UnitsSold += quantity;
                    groupedDailySales[dateKey].Revenue += price * quantity;

                    totalUnitsSold += quantity;
                    totalRevenue += price * quantity;
                }
            }

            var stats = new SalesStatsDto
            {
                DailySales = groupedDailySales.Values.OrderBy(x => x.Date).ToList(),
                UnitsSold = totalUnitsSold,
                TotalRevenue = totalRevenue,
                DailyGoal = 0 // Si tenés lógica para esto, podés calcularlo o parametrizarlo
            };

            return stats;
        }
        public async Task<List<ProductStatsDto>> GetTopProductsAsync(DateTime from, DateTime to, int topN = 5)
        {
            var orders = await FetchOrdersAsync(from, to);

            var productGroups = new Dictionary<string, ProductStatsDto>();

            foreach (var order in orders)
            {
                foreach (var item in order.order_items)
                {
                    var baseTitle = GetBaseProductName(item.item.title);

                    if (!productGroups.ContainsKey(baseTitle))
                    {
                        productGroups[baseTitle] = new ProductStatsDto
                        {
                            ProductName = baseTitle,
                            UnitsSold = 0
                        };
                    }

                    productGroups[baseTitle].TotalUnits += item.quantity;
                }
            }

            return productGroups.Values
                .OrderByDescending(p => p.UnitsSold)
                .Take(topN)
                .ToList();
        }

        private string GetBaseProductName(string fullTitle)
        {
            // Elimina talla o variaciones si están separadas por "-"
            return fullTitle.Split('-')[0].Trim();
        }

        private async Task<List<dynamic>> FetchOrdersAsync(DateTime from, DateTime to)
        {
            // Aquí deberías inyectar el access_token válido (por ahora lo dejamos vacío)
            var accessToken = "TEST123456789abcdefghABCDEFG";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var allOrders = new List<dynamic>();
            int offset = 0;
            const int limit = 50;
            bool hasMore = true;

            while (hasMore)
            {
                var url = $"{_settings.ApiBaseUrl}/orders/search?seller={_settings.SellerId}" +
                          $"&order.status=paid&order.date_created.from={from:yyyy-MM-dd}T00:00:00Z" +
                          $"&order.date_created.to={to:yyyy-MM-dd}T23:59:59Z" +
                          $"&limit={limit}&offset={offset}";

                var response = await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                using var jsonDoc = await JsonDocument.ParseAsync(stream);

                var results = jsonDoc.RootElement.GetProperty("results").EnumerateArray();

                int count = 0;
                foreach (var item in results)
                {
                    allOrders.Add(JsonSerializer.Deserialize<dynamic>(item.GetRawText()));
                    count++;
                }

                hasMore = count == limit;
                offset += limit;
            }

            return allOrders;
        }
    }
}
