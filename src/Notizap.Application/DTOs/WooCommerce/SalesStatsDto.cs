public class SalesStatsDto
{
  public int Year { get; set; }
  public int Month { get; set; }
  public int UnitsSold { get; set; }
  public decimal TotalRevenue { get; set; }
  public int DailyGoal { get; set; }
  public List<DailySalesDto> DailySales { get; set; } = new();

}