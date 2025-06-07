using Microsoft.AspNetCore.Http;

public class FechasCompraRequest
{
    public IFormFile ArchivoCabecera { get; set; } = null!;
    public IFormFile ArchivoDetalles { get; set; } = null!;
    public string Producto { get; set; } = null!;
}