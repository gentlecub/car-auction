using CarAuction.Application.DTOs.Car;

namespace CarAuction.Application.DTOs.Auction;

public class AuctionDto
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public decimal StartingPrice { get; set; }
    public decimal? ReservePrice { get; set; }
    public decimal MinimumBidIncrement { get; set; }
    public decimal CurrentBid { get; set; }
    public int? CurrentBidderId { get; set; }
    public string? CurrentBidderName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int TotalBids { get; set; }
    public string Status { get; set; } = string.Empty;
    public CarDto Car { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public long RemainingSeconds { get; set; }
}

public class AuctionListDto
{
    public int Id { get; set; }
    public decimal CurrentBid { get; set; }
    public DateTime EndTime { get; set; }
    public int TotalBids { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CarBrand { get; set; } = string.Empty;
    public string CarModel { get; set; } = string.Empty;
    public int CarYear { get; set; }
    public string? PrimaryImage { get; set; }
    public long RemainingSeconds { get; set; }
}
