using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/images")]
public class ImagesController : ControllerBase
{
    private readonly IImageProcessingService _imageProcessingService;
    public ImagesController(IImageProcessingService imageProcessingService)
    {
        _imageProcessingService = imageProcessingService;
    }

    [HttpPost("process")]
    [SwaggerOperation(Summary = "Convierte imagenes a 1600x1600, 72DPI, JPG")]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> ProcessImages(
        [FromForm] List<IFormFile> files,
        [FromForm] int width,
        [FromForm] int height,
        [FromForm] int dpi,
        [FromForm] string format,
        [FromForm] int quality = 90)
    {
        if (files == null || files.Count == 0)
            return BadRequest("No se subieron archivos.");
        if (width <= 0 || height <= 0)
            return BadRequest("Dimensiones inválidas.");
        if (dpi <= 0)
            return BadRequest("DPI inválido.");
        if (string.IsNullOrWhiteSpace(format))
            return BadRequest("Formato requerido.");

        var options = new ImageProcessingOptions
        {
            Width = width,
            Height = height,
            Dpi = dpi,
            Format = format,
            Quality = quality
        };

        var results = await _imageProcessingService.ProcessImagesAsync(files, options);

        // Crear ZIP en memoria
        using var zipStream = new MemoryStream();
        using (var archive = new System.IO.Compression.ZipArchive(zipStream, System.IO.Compression.ZipArchiveMode.Create, true))
        {
            foreach (var result in results)
            {
                var entry = archive.CreateEntry(result.FileName!);
                using var entryStream = entry.Open();
                entryStream.Write(result.Data!, 0, result.Data!.Length);
            }
        }
        zipStream.Position = 0;

        // Devolver el ZIP
        return File(zipStream.ToArray(), "application/zip", "imagenes-procesadas.zip");
    }
}
