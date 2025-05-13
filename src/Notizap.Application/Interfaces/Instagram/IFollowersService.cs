public interface IFollowersService
{
    Task<List<FollowerDayData>> GetFollowersMetricsAsync(string accountName, DateTime from, DateTime to);
}