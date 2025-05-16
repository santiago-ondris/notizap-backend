public class SyncResultDto
{
    public List<string> UpdatedCampaigns   { get; set; } = new();
    public List<string> UnchangedCampaigns { get; set; } = new();
    public int ReportId { get; set; }
}
