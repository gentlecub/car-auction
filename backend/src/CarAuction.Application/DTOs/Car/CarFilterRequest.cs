using CarAuction.Application.DTOs.Common;

namespace CarAuction.Application.DTOs.Car;

public class CarFilterRequest : PaginationRequest
{
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
    public int? MileageFrom { get; set; }
    public int? MileageTo { get; set; }
    public string? FuelType { get; set; }
    public string? Transmission { get; set; }
    public string? Color { get; set; }
    public bool? IsActive { get; set; }
    public bool? HasActiveAuction { get; set; }
}
