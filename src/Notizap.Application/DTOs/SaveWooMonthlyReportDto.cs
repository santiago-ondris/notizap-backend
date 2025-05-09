public class SaveWooMonthlyReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public WooCommerceStore Store { get; set; }
    public int UnitsSold { get; set; }
    public decimal Revenue { get; set; }
    public List<DailySalesDto> DailySales { get; set; } = new();
}