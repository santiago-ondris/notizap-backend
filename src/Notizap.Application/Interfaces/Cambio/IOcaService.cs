public interface IOcaService
{
    Task<OcaEnvioResponseDto> GenerarEtiquetaAsync(int cambioId);
}