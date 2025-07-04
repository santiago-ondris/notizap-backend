public interface IEnvioService
{
    Task<List<EnvioDiarioDto>> ObtenerPorMesAsync(int year, int month);
    Task<EnvioDiarioDto?> ObtenerPorFechaAsync(DateTime fecha);
    Task CrearOActualizarAsync(CreateEnvioDiarioDto dto);
    Task EditarAsync(int id, CreateEnvioDiarioDto dto);
    Task EliminarAsync(int id);
    Task<EnvioResumenMensualDto> ObtenerResumenMensualAsync(int year, int month);
    Task<ResultadoLoteDto> CrearOActualizarLoteAsync(List<CreateEnvioDiarioDto> envios);
}