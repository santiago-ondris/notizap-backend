using Microsoft.EntityFrameworkCore;

public class NotizapDbContext : DbContext
    {
        public NotizapDbContext(DbContextOptions<NotizapDbContext> options) : base(options) { }

        public DbSet<Gasto> Gastos { get; set; }
        public DbSet<MercadoLibreManualReport> MercadoLibreManualReports { get; set; }
        public DbSet<WooCommerceMonthlyReport> WooCommerceMonthlyReports { get; set; }
        public DbSet<WooDailySale> WooDailySales { get; set; }
        public DbSet<InstagramReel> InstagramReels => Set<InstagramReel>();
        public DbSet<InstagramStory> InstagramStories => Set<InstagramStory>();
        public DbSet<InstagramPost> InstagramPosts => Set<InstagramPost>();
        public DbSet<CampaignMailchimp> CampaignMailchimps => Set<CampaignMailchimp>();
        public DbSet<EnvioDiario> EnviosDiarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new InstagramReelConfiguration());
            modelBuilder.ApplyConfiguration(new InstagramStoryConfiguration());
            modelBuilder.ApplyConfiguration(new InstagramPostConfiguration());

            // Relación WooCommerce Report -> DailySales
            modelBuilder.Entity<WooCommerceMonthlyReport>()
                .HasMany(r => r.DailySales)
                .WithOne(s => s.MonthlyReport)
                .HasForeignKey(s => s.MonthlyReportId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
