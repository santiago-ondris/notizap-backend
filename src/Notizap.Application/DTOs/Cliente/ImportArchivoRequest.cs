using Microsoft.AspNetCore.Http;

public class ImportArchivoRequest
{
    public IFormFile Archivo { get; set; } = null!;
}