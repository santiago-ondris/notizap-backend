using Notizap.Infrastructure.Services;
using Notizap.Services.Analisis;

namespace Notizap.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNotizapServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddHttpClient();
            // Configuración de Settings
            services.Configure<MailchimpSettings>(config.GetSection("Mailchimp"));
            services.Configure<MetricoolSettings>(config.GetSection("Metricool"));
            services.Configure<MetaAdsSettings>(config.GetSection("MetaAds"));
            services.Configure<OcaSettings>(config.GetSection("OcaSettings"));
            services.Configure<OcaOperativasSettings>(config.GetSection("OcaOperativas"));

            // Servicios de negocio
            services.AddScoped<IMailchimpServiceFactory, MailchimpServiceFactory>();
            services.AddScoped<IMailchimpSyncService, MailchimpSyncService>();
            services.AddScoped<IMailchimpQueryService, MailchimpQueryService>();
            services.AddScoped<IMercadoLibreService, MercadoLibreService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEnvioService, EnvioService>();
            services.AddScoped<IMercadoLibrePublicidadService, MercadoLibrePublicidadService>();
            services.AddScoped<IMercadoLibreExcelProcessor, MercadoLibreExcelProcessor>();
            services.AddScoped<ICambioService, CambioService>();
            services.AddScoped<IDevolucionService, DevolucionService>();
            services.AddScoped<IDevolucionMercadoLibreService, DevolucionMercadoLibreService>();

            services.AddScoped<IComprasMergeService, ComprasMergeService>();
            services.AddScoped<AnalisisRotacionService>();
            services.AddScoped<IEvolucionStockService, EvolucionStockService>();
            services.AddScoped<IEvolucionVentasService, EvolucionVentasService>();

            services.AddScoped<IClienteImportService, ClienteImportService>();
            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<IPlantillaWhatsAppService, PlantillaWhatsAppService>();

            services.AddScoped<IVentaVendedoraService, VentaVendedoraService>();
            services.AddScoped<IRendimientoLocalesService, RendimientoLocalesService>();

            services.AddScoped<IEmailService, EmailService>();

            services.AddScoped<IVentaWooCommerceService, VentaWooCommerceService>();
            services.AddScoped<IComisionOnlineService, ComisionOnlineService>();

            services.AddScoped<IArchivoUsuarioService, ArchivoUsuarioService>();

            return services;
        }
    }
}
