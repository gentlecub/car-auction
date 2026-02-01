using AutoMapper;
using CarAuction.Application.DTOs.Auction;
using CarAuction.Application.DTOs.Auth;
using CarAuction.Application.DTOs.Bid;
using CarAuction.Application.DTOs.Car;
using CarAuction.Application.DTOs.User;
using CarAuction.Domain.Entities;
using System.Text.Json;

namespace CarAuction.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Roles, o => o.MapFrom(s => s.UserRoles.Select(ur => ur.Role.Name)));

        CreateMap<User, UserInfo>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.Roles, o => o.MapFrom(s => s.UserRoles.Select(ur => ur.Role.Name)));

        // Car mappings
        CreateMap<Car, CarDto>()
            .ForMember(d => d.Features, o => o.MapFrom(s =>
                string.IsNullOrEmpty(s.Features) ? null : JsonSerializer.Deserialize<List<string>>(s.Features, (JsonSerializerOptions?)null)));

        CreateMap<CreateCarRequest, Car>()
            .ForMember(d => d.Features, o => o.MapFrom(s =>
                s.Features != null ? JsonSerializer.Serialize(s.Features, (JsonSerializerOptions?)null) : null));

        CreateMap<CarImage, CarImageDto>();

        // Auction mappings
        CreateMap<Auction, AuctionDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.CurrentBidderName, o => o.MapFrom(s => s.CurrentBidder != null ? s.CurrentBidder.FullName : null))
            .ForMember(d => d.RemainingSeconds, o => o.MapFrom(s =>
                s.EndTime > DateTime.UtcNow ? (long)(s.EndTime - DateTime.UtcNow).TotalSeconds : 0));

        CreateMap<Auction, AuctionListDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.CarBrand, o => o.MapFrom(s => s.Car.Brand))
            .ForMember(d => d.CarModel, o => o.MapFrom(s => s.Car.Model))
            .ForMember(d => d.CarYear, o => o.MapFrom(s => s.Car.Year))
            .ForMember(d => d.PrimaryImage, o => o.MapFrom(s =>
                s.Car.Images.FirstOrDefault(i => i.IsPrimary) != null
                    ? s.Car.Images.First(i => i.IsPrimary).ImageUrl
                    : s.Car.Images.FirstOrDefault() != null
                        ? s.Car.Images.First().ImageUrl
                        : null))
            .ForMember(d => d.RemainingSeconds, o => o.MapFrom(s =>
                s.EndTime > DateTime.UtcNow ? (long)(s.EndTime - DateTime.UtcNow).TotalSeconds : 0));

        CreateMap<CreateAuctionRequest, Auction>();

        // Bid mappings
        CreateMap<Bid, BidDto>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.FullName));
    }
}
