using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using Microsoft.AspNetCore.Http;

public class ImageProcessingService : IImageProcessingService
{
    public async Task<List<ProcessedImageResult>> ProcessImagesAsync(
        List<IFormFile> files, ImageProcessingOptions options, CancellationToken cancellationToken = default)
    {
        var tasks = files.Select(async file =>
        {
            using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync<Rgba32>(stream, cancellationToken);

            // Redimensionar al tamaño deseado
            image.Mutate(x => x.Resize(options.Width, options.Height));

            // Setear DPI
            image.Metadata.HorizontalResolution = options.Dpi;
            image.Metadata.VerticalResolution = options.Dpi;

            // Preparar encoder según formato
            IImageEncoder encoder = options.Format!.ToLower() switch
            {
                "jpg" or "jpeg" => new JpegEncoder { Quality = options.Quality > 0 ? options.Quality : 90 },
                "png"           => new PngEncoder(),
                "webp"          => new WebpEncoder { Quality = options.Quality > 0 ? options.Quality : 90 },
                _               => new JpegEncoder { Quality = options.Quality > 0 ? options.Quality : 90 }
            };

            // Guardar en memoria
            using var ms = new MemoryStream();
            await image.SaveAsync(ms, encoder, cancellationToken);

            return new ProcessedImageResult
            {
                FileName = Path.ChangeExtension(file.FileName, options.Format),
                Data = ms.ToArray()
            };
        });

        // Ejecutar todas en paralelo
        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }
}
