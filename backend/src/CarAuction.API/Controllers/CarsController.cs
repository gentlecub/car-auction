using CarAuction.Application.DTOs.Car;
using CarAuction.Application.DTOs.Common;
using CarAuction.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.API.Controllers;

/// <summary>
/// Public endpoints for car information
/// </summary>
[ApiController]
[Route("api/v1/cars")]
public class CarsController : ControllerBase
{
    private readonly ICarService _carService;

    public CarsController(ICarService carService)
    {
        _carService = carService;
    }

    /// <summary>
    /// Get all cars with filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<CarDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResult<CarDto>>>> GetAll([FromQuery] CarFilterRequest request)
    {
        var result = await _carService.GetAllAsync(request);
        return Ok(ApiResponse<PaginatedResult<CarDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get a specific car by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CarDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CarDto>>> GetById(int id)
    {
        var result = await _carService.GetByIdAsync(id);
        return Ok(ApiResponse<CarDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Get list of available car brands
    /// </summary>
    [HttpGet("brands")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetBrands()
    {
        var result = await _carService.GetBrandsAsync();
        return Ok(ApiResponse<IEnumerable<string>>.SuccessResponse(result));
    }
}

/// <summary>
/// Admin endpoints for car management
/// </summary>
[ApiController]
[Route("api/v1/admin/cars")]
[Authorize(Roles = "Admin")]
public class AdminCarsController : ControllerBase
{
    private readonly ICarService _carService;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<AdminCarsController> _logger;

    public AdminCarsController(
        ICarService carService,
        IFileUploadService fileUploadService,
        ILogger<AdminCarsController> logger)
    {
        _carService = carService;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new car
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CarDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CarDto>>> Create([FromBody] CreateCarRequest request)
    {
        var result = await _carService.CreateAsync(request);
        return CreatedAtAction(nameof(CarsController.GetById), "Cars", new { id = result.Id },
            ApiResponse<CarDto>.SuccessResponse(result, "Carro creado exitosamente"));
    }

    /// <summary>
    /// Update an existing car
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CarDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CarDto>>> Update(int id, [FromBody] UpdateCarRequest request)
    {
        var result = await _carService.UpdateAsync(id, request);
        return Ok(ApiResponse<CarDto>.SuccessResponse(result, "Carro actualizado exitosamente"));
    }

    /// <summary>
    /// Delete a car
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        await _carService.DeleteAsync(id);
        return Ok(ApiResponse.CreateSuccess("Carro eliminado exitosamente"));
    }

    /// <summary>
    /// Add an image to a car using URL
    /// </summary>
    [HttpPost("{id}/images")]
    [ProducesResponseType(typeof(ApiResponse<CarImageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CarImageDto>>> AddImage(
        int id,
        [FromBody] AddImageRequest request)
    {
        var result = await _carService.AddImageAsync(id, request.ImageUrl, request.ThumbnailUrl, request.IsPrimary);
        return Ok(ApiResponse<CarImageDto>.SuccessResponse(result, "Imagen agregada exitosamente"));
    }

    /// <summary>
    /// Upload an image file for a car
    /// </summary>
    /// <param name="id">Car ID</param>
    /// <param name="file">Image file (jpg, jpeg, png, gif, webp - max 5MB)</param>
    /// <param name="isPrimary">Set as primary image</param>
    [HttpPost("{id}/images/upload")]
    [ProducesResponseType(typeof(ApiResponse<CarImageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
    public async Task<ActionResult<ApiResponse<CarImageDto>>> UploadImage(
        int id,
        IFormFile file,
        [FromQuery] bool isPrimary = false)
    {
        // Validate file
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse.CreateFail("No se ha proporcionado ningún archivo"));
        }

        if (!_fileUploadService.IsValidImageType(file.FileName, file.ContentType))
        {
            return BadRequest(ApiResponse.CreateFail("Tipo de archivo no permitido. Use: jpg, jpeg, png, gif, webp"));
        }

        if (!_fileUploadService.IsValidFileSize(file.Length))
        {
            return BadRequest(ApiResponse.CreateFail("El archivo excede el tamaño máximo permitido (5MB)"));
        }

        try
        {
            // Upload file
            using var stream = file.OpenReadStream();
            var imageUrl = await _fileUploadService.UploadImageAsync(stream, file.FileName, "cars");

            // Add to car
            var result = await _carService.AddImageAsync(id, imageUrl, null, isPrimary);

            _logger.LogInformation("Image uploaded for car {CarId}: {Url}", id, imageUrl);

            return Ok(ApiResponse<CarImageDto>.SuccessResponse(result, "Imagen subida exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image for car {CarId}", id);
            return BadRequest(ApiResponse.CreateFail("Error al subir la imagen"));
        }
    }

    /// <summary>
    /// Upload multiple images for a car
    /// </summary>
    [HttpPost("{id}/images/upload-multiple")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CarImageDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50MB total limit
    public async Task<ActionResult<ApiResponse<IEnumerable<CarImageDto>>>> UploadMultipleImages(
        int id,
        List<IFormFile> files,
        [FromQuery] int? primaryIndex = null)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest(ApiResponse.CreateFail("No se han proporcionado archivos"));
        }

        if (files.Count > 10)
        {
            return BadRequest(ApiResponse.CreateFail("Máximo 10 imágenes por solicitud"));
        }

        var results = new List<CarImageDto>();
        var errors = new List<string>();

        for (int i = 0; i < files.Count; i++)
        {
            var file = files[i];

            if (!_fileUploadService.IsValidImageType(file.FileName, file.ContentType))
            {
                errors.Add($"{file.FileName}: tipo no permitido");
                continue;
            }

            if (!_fileUploadService.IsValidFileSize(file.Length))
            {
                errors.Add($"{file.FileName}: excede 5MB");
                continue;
            }

            try
            {
                using var stream = file.OpenReadStream();
                var imageUrl = await _fileUploadService.UploadImageAsync(stream, file.FileName, "cars");

                var isPrimary = primaryIndex.HasValue && primaryIndex.Value == i;
                var result = await _carService.AddImageAsync(id, imageUrl, null, isPrimary);
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading {FileName}", file.FileName);
                errors.Add($"{file.FileName}: error al subir");
            }
        }

        var message = errors.Count > 0
            ? $"Subidas {results.Count} imágenes. Errores: {string.Join(", ", errors)}"
            : $"Subidas {results.Count} imágenes exitosamente";

        return Ok(ApiResponse<IEnumerable<CarImageDto>>.SuccessResponse(results, message));
    }

    /// <summary>
    /// Delete a car image
    /// </summary>
    [HttpDelete("{carId}/images/{imageId}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteImage(int carId, int imageId)
    {
        await _carService.DeleteImageAsync(carId, imageId);
        return Ok(ApiResponse.CreateSuccess("Imagen eliminada exitosamente"));
    }

    /// <summary>
    /// Set an image as the primary image for a car
    /// </summary>
    [HttpPut("{carId}/images/{imageId}/primary")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> SetPrimaryImage(int carId, int imageId)
    {
        await _carService.SetPrimaryImageAsync(carId, imageId);
        return Ok(ApiResponse.CreateSuccess("Imagen principal actualizada"));
    }
}

/// <summary>
/// Request to add an image via URL
/// </summary>
public class AddImageRequest
{
    /// <summary>
    /// URL of the image
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Optional thumbnail URL
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Set as primary image
    /// </summary>
    public bool IsPrimary { get; set; }
}
