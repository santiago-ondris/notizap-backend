public interface IPostsService
{
    Task<int> SyncInstagramPostsAsync(string accountName, DateTime from, DateTime to);
    Task<List<InstagramPost>> GetTopPostsAsync(string accountName, DateTime from, DateTime to, string ordenarPor, int limit = 5);
}