using Microsoft.EntityFrameworkCore;

public class ReelsService : IReelsService
    {
        private readonly NotizapDbContext _context;

        public ReelsService(NotizapDbContext context)
        {
            _context = context;
        }

        public async Task<List<Reel>> GetAllAsync()
        {
            return await _context.Reels
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Reel?> GetByIdAsync(int id)
        {
            return await _context.Reels.FindAsync(id);
        }

        public async Task<Reel> CreateAsync(ReelDto dto)
        {
            var reel = new Reel
            {
                Title = dto.Title,
                Description = dto.Description,
                Views = dto.Views,
                Likes = dto.Likes,
                Comments = dto.Comments,
                ThumbnailUrl = dto.ThumbnailUrl,
                CreatedAt = DateTime.UtcNow,
                InstagramUrl = dto.InstagramUrl
            };

            _context.Reels.Add(reel);
            await _context.SaveChangesAsync();

            return reel;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var reel = await _context.Reels.FindAsync(id);
            if (reel is null) return false;

            _context.Reels.Remove(reel);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<Reel?> UpdateAsync(int id, ReelDto dto)
        {
            var existing = await _context.Reels.FindAsync(id);
            if (existing is null) return null;

            existing.Title = dto.Title;
            existing.Description = dto.Description;
            existing.Views = dto.Views;
            existing.Likes = dto.Likes;
            existing.Comments = dto.Comments;
            existing.ThumbnailUrl = dto.ThumbnailUrl;
            existing.InstagramUrl = dto.InstagramUrl;

            await _context.SaveChangesAsync();
            return existing;
        }
        public async Task<List<Reel>> GetTopByViewsAsync(int count)
        {
            return await _context.Reels
                .OrderByDescending(r => r.Views)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Reel>> GetTopByLikesAsync(int count)
        {
            return await _context.Reels
                .OrderByDescending(r => r.Likes)
                .Take(count)
                .ToListAsync();
        }
    }