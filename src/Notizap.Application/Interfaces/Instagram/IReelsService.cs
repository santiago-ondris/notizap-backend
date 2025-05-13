public interface IReelsService
{
    Task<List<ReelMetricDto>> GetInstagramReelsAsync(string accountName, DateTime from, DateTime to);
    Task<int> SyncInstagramReelsAsync(string accountName, DateTime from, DateTime to);
    Task<List<InstagramReel>> GetTopReelsByViewsAsync(string accountName, DateTime from, DateTime to, int limit = 5);
    Task<List<InstagramReel>> GetTopReelsByLikesAsync(string accountName, DateTime from, DateTime to, int limit = 5);
    Task<List<InstagramReel>> GetAllReelsAsync(string accountName, DateTime from, DateTime to);

}