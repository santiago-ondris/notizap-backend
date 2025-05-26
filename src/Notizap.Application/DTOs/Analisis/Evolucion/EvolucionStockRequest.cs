using Microsoft.AspNetCore.Http;

public class EvolucionStockRequest
{
    public IFormFile ArchivoComprasCabecera { get; set; } = null!;
    public IFormFile ArchivoComprasDetalles { get; set; } = null!;
    public IFormFile ArchivoVentas { get; set; } = null!;
    public string Producto { get; set; } = null!;
}
