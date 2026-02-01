namespace CarAuction.Application.DTOs.Bid;

public class BidDto
{
    public int Id { get; set; }
    public int AuctionId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsWinningBid { get; set; }
    public DateTime CreatedAt { get; set; }
}
