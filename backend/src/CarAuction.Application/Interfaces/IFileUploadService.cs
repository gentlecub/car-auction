namespace CarAuction.Application.Interfaces;

/// <summary>
/// Service for handling file uploads (images, documents)
/// </summary>
public interface IFileUploadService
{
    /// <summary>
    /// Upload an image and return the URL
    /// </summary>
    /// <param name="fileStream">The file stream</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="folder">Target folder (e.g., "cars", "users")</param>
    /// <returns>URL of the uploaded file</returns>
    Task<string> UploadImageAsync(Stream fileStream, string fileName, string folder);

    /// <summary>
    /// Upload multiple images
    /// </summary>
    Task<IEnumerable<string>> UploadImagesAsync(IEnumerable<(Stream Stream, string FileName)> files, string folder);

    /// <summary>
    /// Delete an image by URL or path
    /// </summary>
    Task DeleteImageAsync(string imageUrl);

    /// <summary>
    /// Delete multiple images
    /// </summary>
    Task DeleteImagesAsync(IEnumerable<string> imageUrls);

    /// <summary>
    /// Validate that file is an allowed image type
    /// </summary>
    bool IsValidImageType(string fileName, string contentType);

    /// <summary>
    /// Validate file size
    /// </summary>
    bool IsValidFileSize(long fileSize, long maxSizeBytes = 5 * 1024 * 1024); // Default 5MB
}

/// <summary>
/// Result of file upload operation
/// </summary>
public class FileUploadResult
{
    public bool Success { get; set; }
    public string? Url { get; set; }
    public string? Error { get; set; }

    public static FileUploadResult Ok(string url) => new() { Success = true, Url = url };
    public static FileUploadResult Fail(string error) => new() { Success = false, Error = error };
}
