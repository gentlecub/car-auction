namespace CarAuction.Application.DTOs.Bid;

public class BidResponse
{
    public int BidId { get; set; }
    public decimal Amount { get; set; }
    public decimal NewCurrentBid { get; set; }
    public int TotalBids { get; set; }
    public DateTime? NewEndTime { get; set; }
    public bool TimeExtended { get; set; }
}
