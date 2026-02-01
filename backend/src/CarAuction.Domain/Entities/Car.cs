using CarAuction.Domain.Common;

namespace CarAuction.Domain.Entities;

public class Car : BaseEntity
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
    public string? Features { get; set; } // JSON string for features list
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<CarImage> Images { get; set; } = new List<CarImage>();
    public virtual Auction? Auction { get; set; }
}
