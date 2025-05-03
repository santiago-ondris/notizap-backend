using Microsoft.EntityFrameworkCore;

public class NotizapDbContext : DbContext
    {
        public NotizapDbContext(DbContextOptions<NotizapDbContext> options) : base(options) { }

        public DbSet<Reel> Reels => Set<Reel>();
        public DbSet<MercadoLibreManualReport> MercadoLibreManualReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Reel>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Title).IsRequired().HasMaxLength(200);
                entity.Property(r => r.Description).HasMaxLength(1000);
                entity.Property(r => r.Views).IsRequired();
                entity.Property(r => r.Likes).IsRequired();
                entity.Property(r => r.Comments).IsRequired();
                entity.Property(r => r.ThumbnailUrl).IsRequired().HasMaxLength(500);
                entity.Property(r => r.CreatedAt).HasDefaultValueSql("NOW()");
            });
        }
    }
