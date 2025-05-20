public interface IInformeMensualService
{
    Task<byte[]> GenerarInformePdfAsync(int year, int month, bool visual = true);
    Task<ResumenMensualDto> GenerarResumenMensualAsync(int year, int month);
}