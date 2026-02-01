namespace CarAuction.Application.DTOs.Notification;

/// <summary>
/// DTO for notification data
/// </summary>
public class NotificationDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? AuctionId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

/// <summary>
/// Summary of user's notification status
/// </summary>
public class NotificationSummary
{
    public int UnreadCount { get; set; }
    public int TotalCount { get; set; }
    public IEnumerable<NotificationDto> RecentNotifications { get; set; } = Enumerable.Empty<NotificationDto>();
}
