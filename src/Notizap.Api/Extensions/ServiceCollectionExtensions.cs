using Notizap.Application.Ads.Services;
using Notizap.Infrastructure.Ads;
using Notizap.Infrastructure.Services;
using Notizap.Infrastructure.Services.Publicidad;
using NotiZap.Dashboard.API.Services;

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
            services.AddScoped<IWooCommerceService, WooCommerceService>();
            services.AddScoped<IMercadoLibreService, MercadoLibreService>();
            services.AddScoped<IReelsService, ReelsService>();
            services.AddScoped<IImageUploadService, ImageUploadService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IGastoService, GastoService>();
            services.AddScoped<IFollowersService, FollowersService>();
            services.AddScoped<IStoriesService, StoriesService>();
            services.AddScoped<IPostsService, PostsService>();
            services.AddScoped<IEnvioService, EnvioService>();
            services.AddScoped<IAdService, AdService>();
            services.AddScoped<IMetaAdsService, MetaAdsService>();
            services.AddScoped<IMixedAdsService, MixedAdsService>();
            services.AddScoped<IMercadoLibrePublicidadService, MercadoLibrePublicidadService>();
            services.AddScoped<IMercadoLibreExcelProcessor, MercadoLibreExcelProcessor>();
            services.AddScoped<ICambioService, CambioService>();
            services.AddScoped<IDevolucionService, DevolucionService>();
            services.AddScoped<IOcaService, OcaService>();

            // Servicios para el informe mensual
            services.AddScoped<WooResumenBuilder>();
            services.AddScoped<MercadoLibreResumenBuilder>();
            services.AddScoped<InstagramResumenBuilder>();
            services.AddScoped<PublicidadResumenBuilder>();
            services.AddScoped<MailingResumenBuilder>();
            services.AddScoped<GastosResumenBuilder>();
            services.AddScoped<EnviosResumenBuilder>();
            services.AddScoped<CambiosResumenBuilder>();
            services.AddScoped<IInformeMensualService, InformeMensualService>();

            return services;
        }
    }
}
