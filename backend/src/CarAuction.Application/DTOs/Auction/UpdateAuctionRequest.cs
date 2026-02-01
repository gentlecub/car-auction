namespace CarAuction.Application.DTOs.Auction;

public class UpdateAuctionRequest
{
    public decimal? StartingPrice { get; set; }
    public decimal? ReservePrice { get; set; }
    public decimal? MinimumBidIncrement { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? ExtensionMinutes { get; set; }
    public int? ExtensionThresholdMinutes { get; set; }
}
