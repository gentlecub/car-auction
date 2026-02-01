namespace CarAuction.Application.DTOs.Bid;

public class CreateBidRequest
{
    public int AuctionId { get; set; }
    public decimal Amount { get; set; }
}
