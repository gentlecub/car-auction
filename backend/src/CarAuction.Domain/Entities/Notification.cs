using CarAuction.Domain.Common;
using CarAuction.Domain.Enums;

namespace CarAuction.Domain.Entities;

public class Notification : BaseEntity
{
    public int UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? AuctionId { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Auction? Auction { get; set; }
}
