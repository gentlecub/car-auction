using CarAuction.Application.DTOs.Common;
using CarAuction.Application.DTOs.Notification;
using CarAuction.Domain.Enums;

namespace CarAuction.Application.Interfaces;

/// <summary>
/// Service for managing user notifications
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Create a notification for a user
    /// </summary>
    Task CreateNotificationAsync(int userId, NotificationType type, string title, string message, int? auctionId = null);

    /// <summary>
    /// Notify user that they've been outbid
    /// </summary>
    Task NotifyOutbidAsync(int previousBidderId, int auctionId, decimal newBidAmount);

    /// <summary>
    /// Notify user that they won an auction
    /// </summary>
    Task NotifyAuctionWonAsync(int winnerId, int auctionId, decimal finalPrice);

    /// <summary>
    /// Notify bidders that an auction is ending soon
    /// </summary>
    Task NotifyAuctionEndingSoonAsync(int auctionId);

    /// <summary>
    /// Get user's notifications with pagination
    /// </summary>
    Task<PaginatedResult<NotificationDto>> GetUserNotificationsAsync(int userId, PaginationRequest request);

    /// <summary>
    /// Get notification summary (unread count, recent notifications)
    /// </summary>
    Task<NotificationSummary> GetSummaryAsync(int userId);

    /// <summary>
    /// Get a specific notification
    /// </summary>
    Task<NotificationDto?> GetByIdAsync(int userId, int notificationId);

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    Task MarkAsReadAsync(int userId, int notificationId);

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    Task MarkAllAsReadAsync(int userId);

    /// <summary>
    /// Get count of unread notifications
    /// </summary>
    Task<int> GetUnreadCountAsync(int userId);

    /// <summary>
    /// Delete a notification
    /// </summary>
    Task DeleteAsync(int userId, int notificationId);

    /// <summary>
    /// Delete all read notifications older than specified days
    /// </summary>
    Task DeleteOldNotificationsAsync(int userId, int daysOld = 30);
}
