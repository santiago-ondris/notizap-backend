using AutoMapper;

namespace Notizap.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Gasto, GastoDto>();
            CreateMap<CreateGastoDto, Gasto>();
            CreateMap<UpdateGastoDto, Gasto>();

            CreateMap<EnvioDiario, EnvioDiarioDto>()
                .ForMember(dest => dest.TotalCordobaCapital, opt => opt.MapFrom(src => src.TotalCordobaCapital))
                .ForMember(dest => dest.TotalEnvios, opt => opt.MapFrom(src => src.TotalEnvios));

            CreateMap<CreateEnvioDiarioDto, EnvioDiario>();
        }
    }
}
