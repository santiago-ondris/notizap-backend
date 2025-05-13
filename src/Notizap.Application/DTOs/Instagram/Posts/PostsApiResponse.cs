public class PostsApiResponse
{
    public object Metadata { get; set; } = default!;
    public PageDto Page { get; set; } = default!;
    public List<PostMetricDto> Data { get; set; } = new();
}