using Microsoft.AspNetCore.Http;

public class SaveExcelTopDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public IFormFile Archivo { get; set; } = null!;
}
