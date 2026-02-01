using CarAuction.Domain.Common;

namespace CarAuction.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;

    // Navigation properties
    public virtual User User { get; set; } = null!;
}
