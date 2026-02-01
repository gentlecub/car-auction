using CarAuction.Domain.Common;

namespace CarAuction.Domain.Entities;

public class AuctionHistory : BaseEntity
{
    public int AuctionId { get; set; }
    public int? WinnerId { get; set; }
    public decimal? FinalPrice { get; set; }
    public int TotalBids { get; set; }
    public int UniqueParticipants { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool ReserveMet { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Auction Auction { get; set; } = null!;
    public virtual User? Winner { get; set; }
}
