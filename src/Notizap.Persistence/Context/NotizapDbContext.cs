using Microsoft.EntityFrameworkCore;

public class NotizapDbContext : DbContext
{
    public NotizapDbContext(DbContextOptions<NotizapDbContext> options) : base(options) { }

    public DbSet<MercadoLibreManualReport> MercadoLibreManualReports { get; set; }
    public DbSet<CampaignMailchimp> CampaignMailchimps => Set<CampaignMailchimp>();
    public DbSet<EnvioDiario> EnviosDiarios { get; set; }
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
    public DbSet<PlantillaWhatsApp> PlantillasWhatsApp { get; set; }
    public DbSet<DevolucionMercadoLibre> DevolucionesMercadoLibre { get; set; }
    public DbSet<SucursalVenta> SucursalesVentas { get; set; }
    public DbSet<VendedorVenta> VendedoresVentas { get; set; }
    public DbSet<VentaVendedora> VentasVendedoras { get; set; }
    public DbSet<VentaWooCommerce> VentasWooCommerce { get; set; }
    public DbSet<ComisionOnline> ComisionesOnline { get; set; }
    public DbSet<ArchivoUsuario> ArchivosUsuario { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ReportePublicidadMLConfiguration());
        modelBuilder.ApplyConfiguration(new AnuncioDisplayMLConfiguration());
        modelBuilder.ApplyConfiguration(new ExcelTopProductoMLConfiguration());

        modelBuilder.ApplyConfiguration(new ClienteConfiguration());
        modelBuilder.ApplyConfiguration(new CompraConfiguration());
        modelBuilder.ApplyConfiguration(new CompraDetalleConfiguration());
        modelBuilder.ApplyConfiguration(new HistorialImportacionClientesConfiguration());
        modelBuilder.ApplyConfiguration(new PlantillaWhatsAppConfiguration());
        modelBuilder.ApplyConfiguration(new DevolucionMercadoLibreConfiguration());

        modelBuilder.ApplyConfiguration(new SucursalVentaConfiguration());
        modelBuilder.ApplyConfiguration(new VendedorVentaConfiguration());
        modelBuilder.ApplyConfiguration(new VentaVendedoraConfiguration());

        modelBuilder.ApplyConfiguration(new VentaWooCommerceConfiguration());
        modelBuilder.ApplyConfiguration(new ComisionOnlineConfiguration());

        modelBuilder.ApplyConfiguration(new ArchivoUsuarioConfiguration());
    }
}
