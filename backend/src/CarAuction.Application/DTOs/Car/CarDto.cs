namespace CarAuction.Application.DTOs.Car;

public class CarDto
{
    public int Id { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? VIN { get; set; }
    public int Mileage { get; set; }
    public string? Color { get; set; }
    public string? EngineType { get; set; }
    public string? Transmission { get; set; }
    public string? FuelType { get; set; }
    public int? Horsepower { get; set; }
    public string? Description { get; set; }
    public string? Condition { get; set; }
    public List<string>? Features { get; set; }
    public bool IsActive { get; set; }
    public List<CarImageDto> Images { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class CarImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}
