namespace CarAuction.Application.DTOs.Auction;

public class CreateAuctionRequest
{
    public int CarId { get; set; }
    public decimal StartingPrice { get; set; }
    public decimal? ReservePrice { get; set; }
    public decimal MinimumBidIncrement { get; set; } = 100;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int ExtensionMinutes { get; set; } = 5;
    public int ExtensionThresholdMinutes { get; set; } = 2;
}
