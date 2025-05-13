public class MetricoolSettings
{
    public string AccessToken { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public Dictionary<string, string> BlogIds { get; set; } = new();
}