using CarAuction.Domain.Common;
using CarAuction.Domain.Enums;

namespace CarAuction.Domain.Entities;

public class Auction : BaseEntity
{
    public int CarId { get; set; }
    public decimal StartingPrice { get; set; }
    public decimal? ReservePrice { get; set; }
    public decimal MinimumBidIncrement { get; set; } = 100;
    public decimal CurrentBid { get; set; }
    public int? CurrentBidderId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime? OriginalEndTime { get; set; }
    public int ExtensionMinutes { get; set; } = 5;
    public int ExtensionThresholdMinutes { get; set; } = 2;
    public int TotalBids { get; set; } = 0;
    public AuctionStatus Status { get; set; } = AuctionStatus.Pending;

    // Navigation properties
    public virtual Car Car { get; set; } = null!;
    public virtual User? CurrentBidder { get; set; }
    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();
    public virtual AuctionHistory? History { get; set; }
}
