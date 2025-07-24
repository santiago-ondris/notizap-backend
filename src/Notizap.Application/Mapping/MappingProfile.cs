using AutoMapper;

namespace Notizap.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // === ENVIOS MAPPING  ===
            CreateMap<EnvioDiario, EnvioDiarioDto>()
                .ForMember(dest => dest.TotalCordobaCapital, opt => opt.MapFrom(src => src.TotalCordobaCapital))
                .ForMember(dest => dest.TotalEnvios, opt => opt.MapFrom(src => src.TotalEnvios));

            CreateMap<CreateEnvioDiarioDto, EnvioDiario>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));


            // === MERCADOLIBRE MAPPING ===
            CreateMap<CreateProductAdDto, ReportePublicidadML>()
                .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => TipoPublicidadML.ProductAds));

            CreateMap<CreateBrandAdDto, ReportePublicidadML>()
                .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => TipoPublicidadML.BrandAds));

            CreateMap<CreateDisplayAdDto, ReportePublicidadML>()
                .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => TipoPublicidadML.DisplayAds));

            CreateMap<DisplayAnuncioDto, AnuncioDisplayML>();  

            // === CAMBIOS Y DEVOLUCIONES MAPPING ===
            CreateMap<CreateDevolucionDto, Devolucion>();
            CreateMap<Devolucion, DevolucionDto>();
            CreateMap<DevolucionDto, Devolucion>();
            CreateMap<CreateCambioSimpleDto, Cambio>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.LlegoAlDeposito, opt => opt.Ignore())
                .ForMember(dest => dest.YaEnviado, opt => opt.Ignore())
                .ForMember(dest => dest.CambioRegistradoSistema, opt => opt.Ignore())
                .ForMember(dest => dest.EtiquetaDespachada, opt => opt.Ignore());
                
            CreateMap<Cambio, CambioSimpleDto>()
                .ForMember(dest => dest.Etiqueta, opt => opt.MapFrom(src => src.Etiqueta))
                .ForMember(dest => dest.EtiquetaDespachada, opt => opt.MapFrom(src => src.EtiquetaDespachada));

            // === ARCHIVOS USUARIO MAPPING === 
            CreateMap<ArchivoUsuario, ArchivoUsuarioDto>()
                .ForMember(dest => dest.TipoArchivoTexto, opt => opt.Ignore()) // Se calcula en el servicio
                .ForMember(dest => dest.TamañoFormateado, opt => opt.Ignore()) // Se calcula en el servicio
                .ForMember(dest => dest.Tags, opt => opt.Ignore()) // Se procesa en el servicio
                .ForMember(dest => dest.UsuarioNombre, opt => opt.Ignore()); // Se calcula en el servicio

            CreateMap<CrearArchivoUsuarioDto, ArchivoUsuario>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioId, opt => opt.Ignore())
                .ForMember(dest => dest.NombreOriginal, opt => opt.Ignore())
                .ForMember(dest => dest.RutaArchivo, opt => opt.Ignore())
                .ForMember(dest => dest.FechaSubida, opt => opt.Ignore())
                .ForMember(dest => dest.TamañoBytes, opt => opt.Ignore())
                .ForMember(dest => dest.UltimoAcceso, opt => opt.Ignore())
                .ForMember(dest => dest.VecesUtilizado, opt => opt.Ignore())
                .ForMember(dest => dest.Activo, opt => opt.Ignore())
                .ForMember(dest => dest.TagsMetadata, opt => opt.Ignore())
                .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
                .ForMember(dest => dest.FechaModificacion, opt => opt.Ignore())
                .ForMember(dest => dest.CreadoPor, opt => opt.Ignore())
                .ForMember(dest => dest.ModificadoPor, opt => opt.Ignore())
                .ForMember(dest => dest.Usuario, opt => opt.Ignore());    
        }
                    
    }
}