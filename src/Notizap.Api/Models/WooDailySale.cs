using System.Text.Json.Serialization;

public class WooDailySale
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public int UnitsSold { get; set; }
    public decimal Revenue { get; set; }

    // FK con la tabla mensual
    public int MonthlyReportId { get; set; }
    [JsonIgnore]
    public WooCommerceMonthlyReport? MonthlyReport { get; set; }
}