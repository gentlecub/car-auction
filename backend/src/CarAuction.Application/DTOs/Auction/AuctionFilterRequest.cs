using CarAuction.Application.DTOs.Common;

namespace CarAuction.Application.DTOs.Auction;

public class AuctionFilterRequest : PaginationRequest
{
    public string? Status { get; set; }
    public string? Brand { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? EndingSoon { get; set; }
}
