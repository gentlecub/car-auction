using CarAuction.Application.DTOs.Car;
using CarAuction.Application.DTOs.Common;
using CarAuction.IntegrationTests.Fixtures;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CarAuction.IntegrationTests.Controllers;

public class CarImageUploadTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public CarImageUploadTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var loginRequest = new { Email = "admin@test.com", Password = "Admin123!" };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        return result!.Data!.AccessToken;
    }

    [Fact]
    public async Task UploadImage_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var content = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(CreateFakeImageBytes());
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(imageContent, "file", "test.jpg");

        // Act
        var response = await _client.PostAsync("/api/v1/admin/cars/1/images/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadImage_WithInvalidFileType_ReturnsBadRequest()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x00, 0x01, 0x02 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", "document.pdf");

        // Act
        var response = await _client.PostAsync("/api/v1/admin/cars/1/images/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result!.Message.Should().Contain("tipo de archivo no permitido");
    }

    [Fact]
    public async Task UploadImage_WithNoFile_ReturnsBadRequest()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();

        // Act
        var response = await _client.PostAsync("/api/v1/admin/cars/1/images/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadMultipleImages_ExceedsLimit_ReturnsBadRequest()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();

        // Add 11 files (exceeds 10 limit)
        for (int i = 0; i < 11; i++)
        {
            var imageContent = new ByteArrayContent(CreateFakeImageBytes());
            imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            content.Add(imageContent, "files", $"image{i}.jpg");
        }

        // Act
        var response = await _client.PostAsync("/api/v1/admin/cars/1/images/upload-multiple", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result!.Message.Should().Contain("Máximo 10 imágenes");
    }

    [Fact]
    public async Task AddImageByUrl_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            ImageUrl = "https://example.com/car-image.jpg",
            ThumbnailUrl = "https://example.com/car-image-thumb.jpg",
            IsPrimary = false
        };

        // Act - using car ID 1 from seed data
        var response = await _client.PostAsJsonAsync("/api/v1/admin/cars/1/images", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CarImageDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.ImageUrl.Should().Be(request.ImageUrl);
    }

    [Fact]
    public async Task DeleteImage_WithValidIds_ReturnsSuccess()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // First add an image
        var addRequest = new
        {
            ImageUrl = "https://example.com/delete-test.jpg",
            IsPrimary = false
        };

        var addResponse = await _client.PostAsJsonAsync("/api/v1/admin/cars/1/images", addRequest);
        var addResult = await addResponse.Content.ReadFromJsonAsync<ApiResponse<CarImageDto>>();
        var imageId = addResult!.Data!.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/v1/admin/cars/1/images/{imageId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result!.Success.Should().BeTrue();
    }

    private static byte[] CreateFakeImageBytes()
    {
        // Create minimal valid JPEG header bytes
        return new byte[]
        {
            0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01,
            0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0xFF, 0xD9
        };
    }

    // Helper class for auth response deserialization
    private class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
