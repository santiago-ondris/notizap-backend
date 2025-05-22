using Microsoft.AspNetCore.Http;

public interface IImageProcessingService
{
    Task<List<ProcessedImageResult>> ProcessImagesAsync(
        List<IFormFile> files,
        ImageProcessingOptions options,
        CancellationToken cancellationToken = default);
}
