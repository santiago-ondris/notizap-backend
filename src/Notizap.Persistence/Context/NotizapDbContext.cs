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
        public DbSet<AdReport> AdReports { get; set; }
        public DbSet<AdCampaign> AdCampaigns { get; set; }
        public DbSet<ReportePublicidadML> ReportesPublicidadML { get; set; }
        public DbSet<AnuncioDisplayML> AnunciosDisplayML { get; set; }
        public DbSet<ExcelTopProductoML> ExcelTopProductosML { get; set; }
        public DbSet<Cambio> Cambios { get; set; }
        public DbSet<Devolucion> Devoluciones { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<CompraDetalle> CompraDetalles { get; set; }
        public DbSet<HistorialImportacionClientes> HistorialImportacionClientes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new InstagramReelConfiguration());
            modelBuilder.ApplyConfiguration(new InstagramStoryConfiguration());
            modelBuilder.ApplyConfiguration(new InstagramPostConfiguration());

            modelBuilder.ApplyConfiguration(new AdReportConfiguration());
            modelBuilder.ApplyConfiguration(new AdCampaignConfiguration());

            modelBuilder.ApplyConfiguration(new ReportePublicidadMLConfiguration());
            modelBuilder.ApplyConfiguration(new AnuncioDisplayMLConfiguration());
            modelBuilder.ApplyConfiguration(new ExcelTopProductoMLConfiguration());

            modelBuilder.ApplyConfiguration(new ClienteConfiguration());
            modelBuilder.ApplyConfiguration(new CompraConfiguration());
            modelBuilder.ApplyConfiguration(new CompraDetalleConfiguration());
            modelBuilder.ApplyConfiguration(new HistorialImportacionClientesConfiguration());

            // Relación WooCommerce Report -> DailySales
            modelBuilder.Entity<WooCommerceMonthlyReport>()
                .HasMany(r => r.DailySales)
                .WithOne(s => s.MonthlyReport)
                .HasForeignKey(s => s.MonthlyReportId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
