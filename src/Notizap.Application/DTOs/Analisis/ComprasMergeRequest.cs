using Microsoft.AspNetCore.Http;

public class ComprasMergeRequest
{
    public IFormFile? ArchivoComprasCabecera { get; set; }
    public IFormFile? ArchivoComprasDetalles { get; set; }
}