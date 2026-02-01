using CarAuction.Domain.Common;

namespace CarAuction.Domain.Entities;

public class CarImage : BaseEntity
{
    public int CarId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public bool IsPrimary { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;

    // Navigation properties
    public virtual Car Car { get; set; } = null!;
}
