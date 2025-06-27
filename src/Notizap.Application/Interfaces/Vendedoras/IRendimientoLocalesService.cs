public interface IRendimientoLocalesService
{
    Task<RendimientoLocalesResponseDto> ObtenerRendimientoLocalesAsync(RendimientoLocalesFilterDto filtros);
}
