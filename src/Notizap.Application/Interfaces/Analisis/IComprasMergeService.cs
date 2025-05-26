using Microsoft.AspNetCore.Http;

public interface IComprasMergeService
{
    Task<List<CompraDetalleConFechaDto>> MergeComprasConDetallesAsync(
        IFormFile archivoComprasCabecera,
        IFormFile archivoComprasDetalles
    );
}