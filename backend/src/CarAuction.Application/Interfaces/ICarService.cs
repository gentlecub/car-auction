using CarAuction.Application.DTOs.Car;
using CarAuction.Application.DTOs.Common;

namespace CarAuction.Application.Interfaces;

public interface ICarService
{
    Task<CarDto> GetByIdAsync(int id);
    Task<PaginatedResult<CarDto>> GetAllAsync(CarFilterRequest request);
    Task<CarDto> CreateAsync(CreateCarRequest request);
    Task<CarDto> UpdateAsync(int id, UpdateCarRequest request);
    Task DeleteAsync(int id);
    Task<CarImageDto> AddImageAsync(int carId, string imageUrl, string? thumbnailUrl, bool isPrimary);
    Task DeleteImageAsync(int carId, int imageId);
    Task SetPrimaryImageAsync(int carId, int imageId);
    Task<IEnumerable<string>> GetBrandsAsync();
}
