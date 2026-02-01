using CarAuction.Application.DTOs.Bid;

namespace CarAuction.Application.Interfaces;

/// <summary>
/// Service for sending real-time notifications via SignalR
/// </summary>
public interface IRealTimeNotificationService
{
    /// <summary>
    /// Notify all clients watching an auction about a new bid
    /// </summary>
    Task NotifyNewBidAsync(int auctionId, BidResponse bidResponse);

    /// <summary>
    /// Notify all clients that an auction has ended
    /// </summary>
    Task NotifyAuctionClosedAsync(int auctionId, int? winnerId, decimal finalPrice);

    /// <summary>
    /// Notify all clients that an auction time was extended
    /// </summary>
    Task NotifyTimeExtendedAsync(int auctionId, DateTime newEndTime);

    /// <summary>
    /// Notify all clients watching an auction that it's ending soon
    /// </summary>
    Task NotifyAuctionEndingSoonAsync(int auctionId, int minutesRemaining);

    /// <summary>
    /// Send a personal notification to a specific user
    /// </summary>
    Task SendUserNotificationAsync(int userId, string type, string title, string message);

    /// <summary>
    /// Notify clients that multiple auctions have been closed
    /// </summary>
    Task NotifyAuctionsClosedBatchAsync(int count);
}
