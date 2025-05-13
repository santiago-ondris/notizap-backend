public interface IStoriesService
{
    Task<int> SyncInstagramStoriesAsync(string accountName, DateTime from, DateTime to);
    Task<List<InstagramStory>> GetTopStoriesAsync(string accountName, DateTime from, DateTime to, string criterio, int limit = 5);
}