namespace test2.Services;

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;

    public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
    {
        _logger = logger;

        // Lấy cấu hình trực tiếp từ appsettings.json
        var account = new Account(
            configuration["CloudinarySettings:CloudName"],
            configuration["CloudinarySettings:ApiKey"],
            configuration["CloudinarySettings:ApiSecret"]
        );

        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogError("File upload failed: File is null or empty.");
                return null;
            }

            using (var stream = file.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream)
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult?.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    _logger.LogError($"File upload to Cloudinary failed: {uploadResult.Error?.Message}");
                    return null;
                }

                return uploadResult.SecureUrl?.ToString(); // Trả về link ảnh hoặc null nếu không có SecureUrl
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception during Cloudinary upload: {ex.Message}");
            return null;
        }
    }
}