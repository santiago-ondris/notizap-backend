using AutoMapper;
using Notizap.Application.Ads.Dtos;

namespace Notizap.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<MetaCampaignInsightDto, MixedCampaignInsightDto>();

            CreateMap<Gasto, GastoDto>();
            CreateMap<CreateGastoDto, Gasto>();
            CreateMap<UpdateGastoDto, Gasto>();

            CreateMap<EnvioDiario, EnvioDiarioDto>()
                .ForMember(dest => dest.TotalCordobaCapital, opt => opt.MapFrom(src => src.TotalCordobaCapital))
                .ForMember(dest => dest.TotalEnvios,          opt => opt.MapFrom(src => src.TotalEnvios));

            CreateMap<CreateEnvioDiarioDto, EnvioDiario>();

            // Reportes de publicidad
            CreateMap<AdReport, AdReportDto>();
            CreateMap<SaveAdReportDto, AdReport>();

            // AdCampaign ↔ DTOs con FollowersCount y CampaignId
            CreateMap<AdCampaign, AdCampaignReadDto>()
                .ForMember(dest => dest.CampaignId,    opt => opt.MapFrom(src => src.CampaignId))
                .ForMember(dest => dest.FollowersCount,opt => opt.MapFrom(src => src.FollowersCount));

            CreateMap<AdCampaign, AdCampaignDto>()
                .ForMember(dest => dest.CampaignId,    opt => opt.MapFrom(src => src.CampaignId))
                .ForMember(dest => dest.FollowersCount,opt => opt.MapFrom(src => src.FollowersCount));

            CreateMap<AdCampaignDto, AdCampaign>()
                .ForMember(dest => dest.CampaignId,    opt => opt.MapFrom(src => src.CampaignId))
                .ForMember(dest => dest.FollowersCount,opt => opt.MapFrom(src => src.FollowersCount));

            CreateMap<MetaCampaignInsightDto, AdCampaign>()
                .ForMember(dest => dest.AdReportId,     opt => opt.Ignore())
                .ForMember(dest => dest.Reporte,        opt => opt.Ignore())

                .ForMember(dest => dest.CampaignId,     opt => opt.MapFrom(src => src.CampaignId))
                .ForMember(dest => dest.Nombre,         opt => opt.MapFrom(src => src.CampaignName))
                .ForMember(dest => dest.Tipo,           opt => opt.MapFrom(src => src.Objective))
                .ForMember(dest => dest.Objetivo,       opt => opt.MapFrom(src => src.Objective))
                .ForMember(dest => dest.MontoInvertido, opt => opt.MapFrom(src => src.Spend))
                .ForMember(dest => dest.Resultados,     opt => opt.MapFrom(src => src.ResultadoPrincipal))

                .ForMember(dest => dest.Clicks,         opt => opt.MapFrom(src => src.Clicks))
                .ForMember(dest => dest.Impressions,    opt => opt.MapFrom(src => src.Impressions))
                .ForMember(dest => dest.Ctr,            opt => opt.MapFrom(src => src.Ctr))
                .ForMember(dest => dest.Reach,          opt => opt.MapFrom(src => src.Reach))
                .ForMember(dest => dest.ValorResultado, opt => opt.MapFrom(src => src.ValorResultado))

                .ForMember(dest => dest.FechaInicio,    opt => opt.MapFrom(src => src.Start))
                .ForMember(dest => dest.FechaFin,       opt => opt.MapFrom(src => src.End))

                .ForMember(dest => dest.FollowersCount, opt => opt.Ignore());

            CreateMap<CreateProductAdDto, ReportePublicidadML>()
                .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => TipoPublicidadML.ProductAds));

            CreateMap<CreateBrandAdDto, ReportePublicidadML>()
                .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => TipoPublicidadML.BrandAds));

            CreateMap<CreateDisplayAdDto, ReportePublicidadML>()
                .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => TipoPublicidadML.DisplayAds));

            CreateMap<DisplayAnuncioDto, AnuncioDisplayML>();   
        }
                    
    }
}
