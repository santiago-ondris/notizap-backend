public class StoriesApiResponse
{
    public object Metadata { get; set; } = default!;
    public PageDto Page { get; set; } = default!;
    public List<StoryMetricDto> Data { get; set; } = new();
}
