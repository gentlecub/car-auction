using CarAuction.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CarAuction.Infrastructure.Services;

/// <summary>
/// Local file system implementation for file uploads
/// For production, consider using cloud storage (AWS S3, Azure Blob, etc.)
/// </summary>
public class LocalFileUploadService : IFileUploadService
{
    private readonly ILogger<LocalFileUploadService> _logger;
    private readonly string _uploadPath;
    private readonly string _baseUrl;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp"
    };

    public LocalFileUploadService(IConfiguration configuration, ILogger<LocalFileUploadService> logger)
    {
        _logger = logger;
        _uploadPath = configuration["FileUpload:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        _baseUrl = configuration["FileUpload:BaseUrl"] ?? "/uploads";

        // Ensure upload directory exists
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string folder)
    {
        try
        {
            // Generate unique file name
            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";

            // Create folder path
            var folderPath = Path.Combine(_uploadPath, folder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(folderPath, uniqueFileName);

            // Save file
            using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOutput);
            }

            var url = $"{_baseUrl}/{folder}/{uniqueFileName}";
            _logger.LogInformation("Image uploaded: {Url}", url);

            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image: {FileName}", fileName);
            throw;
        }
    }

    public async Task<IEnumerable<string>> UploadImagesAsync(IEnumerable<(Stream Stream, string FileName)> files, string folder)
    {
        var urls = new List<string>();

        foreach (var (stream, fileName) in files)
        {
            var url = await UploadImageAsync(stream, fileName, folder);
            urls.Add(url);
        }

        return urls;
    }

    public Task DeleteImageAsync(string imageUrl)
    {
        try
        {
            // Convert URL to file path
            var relativePath = imageUrl.Replace(_baseUrl, "").TrimStart('/');
            var filePath = Path.Combine(_uploadPath, relativePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Image deleted: {Url}", imageUrl);
            }
            else
            {
                _logger.LogWarning("Image not found for deletion: {Url}", imageUrl);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image: {Url}", imageUrl);
        }

        return Task.CompletedTask;
    }

    public async Task DeleteImagesAsync(IEnumerable<string> imageUrls)
    {
        foreach (var url in imageUrls)
        {
            await DeleteImageAsync(url);
        }
    }

    public bool IsValidImageType(string fileName, string contentType)
    {
        var extension = Path.GetExtension(fileName);

        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        return AllowedExtensions.Contains(extension) && AllowedContentTypes.Contains(contentType);
    }

    public bool IsValidFileSize(long fileSize, long maxSizeBytes = 5 * 1024 * 1024)
    {
        return fileSize > 0 && fileSize <= maxSizeBytes;
    }
}
