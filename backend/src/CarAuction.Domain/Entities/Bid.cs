using CarAuction.Domain.Common;

namespace CarAuction.Domain.Entities;

public class Bid : BaseEntity
{
    public int AuctionId { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public bool IsWinningBid { get; set; } = false;
    public string? IpAddress { get; set; }

    // Navigation properties
    public virtual Auction Auction { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
