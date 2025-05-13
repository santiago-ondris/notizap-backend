public class ReelsApiResponse
{
    public object Metadata { get; set; } = default!;
    public PageDto Page { get; set; } = default!;
    public List<ReelMetricDto> Data { get; set; } = new();
}

public class PageDto
{
    public string? Next { get; set; }
}