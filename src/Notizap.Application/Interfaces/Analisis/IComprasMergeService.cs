using Microsoft.AspNetCore.Http;

public interface IComprasMergeService
{
    List<CompraDetalleConFechaDto> MergeComprasConDetalles(
        IFormFile archivoComprasCabecera,
        IFormFile archivoComprasDetalles
    );
}