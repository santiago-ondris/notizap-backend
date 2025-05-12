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
        }
    }
}
