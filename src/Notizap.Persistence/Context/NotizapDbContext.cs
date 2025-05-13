using Microsoft.EntityFrameworkCore;

public class NotizapDbContext : DbContext
    {
        public NotizapDbContext(DbContextOptions<NotizapDbContext> options) : base(options) { }

        public DbSet<Reel> Reels => Set<Reel>();
        public DbSet<Gasto> Gastos { get; set; }
        public DbSet<MercadoLibreManualReport> MercadoLibreManualReports { get; set; }
        public DbSet<WooCommerceMonthlyReport> WooCommerceMonthlyReports { get; set; }
        public DbSet<WooDailySale> WooDailySales { get; set; }
        public DbSet<InstagramReel> InstagramReels => Set<InstagramReel>();
        public DbSet<InstagramStory> InstagramStories => Set<InstagramStory>();
        public DbSet<InstagramPost> InstagramPosts => Set<InstagramPost>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new InstagramReelConfiguration());
            modelBuilder.ApplyConfiguration(new InstagramStoryConfiguration());
            modelBuilder.ApplyConfiguration(new InstagramPostConfiguration());
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

            // Relación WooCommerce Report -> DailySales
            modelBuilder.Entity<WooCommerceMonthlyReport>()
                .HasMany(r => r.DailySales)
                .WithOne(s => s.MonthlyReport)
                .HasForeignKey(s => s.MonthlyReportId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
