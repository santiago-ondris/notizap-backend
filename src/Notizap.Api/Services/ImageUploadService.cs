using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

public interface IImageUploadService
{
    Task<string> UploadImageAsync(IFormFile file);
}

public class ImageUploadService : IImageUploadService
{
    private readonly Cloudinary _cloudinary;

    public ImageUploadService(IOptions<CloudinarySettings> settings)
    {
        var acc = new Account(
            settings.Value.CloudName,
            settings.Value.ApiKey,
            settings.Value.ApiSecret
        );

        _cloudinary = new Cloudinary(acc);
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "notizap-reels"
        };

        var result = await _cloudinary.UploadAsync(uploadParams);
        return result.SecureUrl.ToString();
    }
}
