namespace CarAuction.Application.DTOs.Car;

public class CreateCarRequest
{
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
}
