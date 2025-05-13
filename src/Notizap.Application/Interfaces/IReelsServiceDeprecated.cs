public interface IReelsServiceDeprecated
{
        Task<List<Reel>> GetAllAsync();
        Task<Reel?> GetByIdAsync(int id);
        Task<Reel> CreateAsync(ReelDto dto);
        Task<Reel?> UpdateAsync(int id, ReelDto dto);
        Task<bool> DeleteAsync(int id);
        Task<List<Reel>> GetTopByViewsAsync(int count);
        Task<List<Reel>> GetTopByLikesAsync(int count);
}