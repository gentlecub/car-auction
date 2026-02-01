using AutoMapper;
using CarAuction.Application.DTOs.Car;
using CarAuction.Application.DTOs.Common;
using CarAuction.Application.Interfaces;
using CarAuction.Domain.Entities;
using CarAuction.Domain.Exceptions;
using CarAuction.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CarAuction.Infrastructure.Services;

public class CarService : ICarService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CarService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CarDto> GetByIdAsync(int id)
    {
        var car = await _context.Cars
            .Include(c => c.Images.OrderBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(c => c.Id == id);

        if (car == null)
        {
            throw new NotFoundException(nameof(Car), id);
        }

        return _mapper.Map<CarDto>(car);
    }

    public async Task<PaginatedResult<CarDto>> GetAllAsync(CarFilterRequest request)
    {
        var query = _context.Cars
            .Include(c => c.Images.OrderBy(i => i.DisplayOrder))
            .Include(c => c.Auction)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.Brand))
            query = query.Where(c => c.Brand.Contains(request.Brand));

        if (!string.IsNullOrEmpty(request.Model))
            query = query.Where(c => c.Model.Contains(request.Model));

        if (request.YearFrom.HasValue)
            query = query.Where(c => c.Year >= request.YearFrom.Value);

        if (request.YearTo.HasValue)
            query = query.Where(c => c.Year <= request.YearTo.Value);

        if (request.MileageFrom.HasValue)
            query = query.Where(c => c.Mileage >= request.MileageFrom.Value);

        if (request.MileageTo.HasValue)
            query = query.Where(c => c.Mileage <= request.MileageTo.Value);

        if (!string.IsNullOrEmpty(request.FuelType))
            query = query.Where(c => c.FuelType == request.FuelType);

        if (!string.IsNullOrEmpty(request.Transmission))
            query = query.Where(c => c.Transmission == request.Transmission);

        if (!string.IsNullOrEmpty(request.Color))
            query = query.Where(c => c.Color == request.Color);

        if (request.IsActive.HasValue)
            query = query.Where(c => c.IsActive == request.IsActive.Value);

        if (request.HasActiveAuction.HasValue)
        {
            if (request.HasActiveAuction.Value)
                query = query.Where(c => c.Auction != null && c.Auction.Status == Domain.Enums.AuctionStatus.Active);
            else
                query = query.Where(c => c.Auction == null || c.Auction.Status != Domain.Enums.AuctionStatus.Active);
        }

        var totalItems = await query.CountAsync();

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "brand" => request.SortDescending ? query.OrderByDescending(c => c.Brand) : query.OrderBy(c => c.Brand),
            "year" => request.SortDescending ? query.OrderByDescending(c => c.Year) : query.OrderBy(c => c.Year),
            "mileage" => request.SortDescending ? query.OrderByDescending(c => c.Mileage) : query.OrderBy(c => c.Mileage),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        var cars = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PaginatedResult<CarDto>
        {
            Items = _mapper.Map<IEnumerable<CarDto>>(cars),
            TotalItems = totalItems,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<CarDto> CreateAsync(CreateCarRequest request)
    {
        var car = _mapper.Map<Car>(request);
        car.IsActive = true;

        _context.Cars.Add(car);
        await _context.SaveChangesAsync();

        return _mapper.Map<CarDto>(car);
    }

    public async Task<CarDto> UpdateAsync(int id, UpdateCarRequest request)
    {
        var car = await _context.Cars
            .Include(c => c.Images)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (car == null)
        {
            throw new NotFoundException(nameof(Car), id);
        }

        if (!string.IsNullOrEmpty(request.Brand)) car.Brand = request.Brand;
        if (!string.IsNullOrEmpty(request.Model)) car.Model = request.Model;
        if (request.Year.HasValue) car.Year = request.Year.Value;
        if (request.VIN != null) car.VIN = request.VIN;
        if (request.Mileage.HasValue) car.Mileage = request.Mileage.Value;
        if (request.Color != null) car.Color = request.Color;
        if (request.EngineType != null) car.EngineType = request.EngineType;
        if (request.Transmission != null) car.Transmission = request.Transmission;
        if (request.FuelType != null) car.FuelType = request.FuelType;
        if (request.Horsepower.HasValue) car.Horsepower = request.Horsepower.Value;
        if (request.Description != null) car.Description = request.Description;
        if (request.Condition != null) car.Condition = request.Condition;
        if (request.Features != null) car.Features = JsonSerializer.Serialize(request.Features);
        if (request.IsActive.HasValue) car.IsActive = request.IsActive.Value;

        await _context.SaveChangesAsync();

        return _mapper.Map<CarDto>(car);
    }

    public async Task DeleteAsync(int id)
    {
        var car = await _context.Cars
            .Include(c => c.Auction)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (car == null)
        {
            throw new NotFoundException(nameof(Car), id);
        }

        if (car.Auction != null && car.Auction.Status == Domain.Enums.AuctionStatus.Active)
        {
            throw new BadRequestException("No se puede eliminar un carro con una subasta activa");
        }

        _context.Cars.Remove(car);
        await _context.SaveChangesAsync();
    }

    public async Task<CarImageDto> AddImageAsync(int carId, string imageUrl, string? thumbnailUrl, bool isPrimary)
    {
        var car = await _context.Cars
            .Include(c => c.Images)
            .FirstOrDefaultAsync(c => c.Id == carId);

        if (car == null)
        {
            throw new NotFoundException(nameof(Car), carId);
        }

        if (isPrimary)
        {
            foreach (var img in car.Images)
            {
                img.IsPrimary = false;
            }
        }

        var image = new CarImage
        {
            CarId = carId,
            ImageUrl = imageUrl,
            ThumbnailUrl = thumbnailUrl,
            IsPrimary = isPrimary || !car.Images.Any(),
            DisplayOrder = car.Images.Count
        };

        _context.CarImages.Add(image);
        await _context.SaveChangesAsync();

        return _mapper.Map<CarImageDto>(image);
    }

    public async Task DeleteImageAsync(int carId, int imageId)
    {
        var image = await _context.CarImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.CarId == carId);

        if (image == null)
        {
            throw new NotFoundException(nameof(CarImage), imageId);
        }

        _context.CarImages.Remove(image);
        await _context.SaveChangesAsync();
    }

    public async Task SetPrimaryImageAsync(int carId, int imageId)
    {
        var car = await _context.Cars
            .Include(c => c.Images)
            .FirstOrDefaultAsync(c => c.Id == carId);

        if (car == null)
        {
            throw new NotFoundException(nameof(Car), carId);
        }

        var image = car.Images.FirstOrDefault(i => i.Id == imageId);
        if (image == null)
        {
            throw new NotFoundException(nameof(CarImage), imageId);
        }

        foreach (var img in car.Images)
        {
            img.IsPrimary = img.Id == imageId;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<string>> GetBrandsAsync()
    {
        return await _context.Cars
            .Where(c => c.IsActive)
            .Select(c => c.Brand)
            .Distinct()
            .OrderBy(b => b)
            .ToListAsync();
    }
}
