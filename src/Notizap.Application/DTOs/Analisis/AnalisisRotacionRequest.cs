using Microsoft.AspNetCore.Http;

public class AnalisisRotacionRequest
{
    public required IFormFile ArchivoComprasCabecera { get; set; }
    public required IFormFile ArchivoComprasDetalles { get; set; }
    public required IFormFile ArchivoVentas { get; set; }
}