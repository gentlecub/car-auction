using CarAuction.API.Hubs;
using CarAuction.Application.DTOs.Bid;
using CarAuction.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CarAuction.API.Services;

/// <summary>
/// Service for sending real-time notifications via SignalR
/// </summary>
public class RealTimeNotificationService : IRealTimeNotificationService
{
    private readonly IHubContext<AuctionHub> _hubContext;
    private readonly ILogger<RealTimeNotificationService> _logger;

    public RealTimeNotificationService(
        IHubContext<AuctionHub> hubContext,
        ILogger<RealTimeNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyNewBidAsync(int auctionId, BidResponse bidResponse)
    {
        var groupName = AuctionHub.GetAuctionGroup(auctionId);

        await _hubContext.Clients.Group(groupName).SendAsync(HubMessages.NewBid, new
        {
            auctionId,
            bidResponse.BidId,
            bidResponse.Amount,
            bidResponse.NewCurrentBid,
            bidResponse.TotalBids,
            bidResponse.NewEndTime,
            bidResponse.TimeExtended,
            Timestamp = DateTime.UtcNow
        });

        _logger.LogDebug(
            "Sent NewBid notification for auction {AuctionId}: {Amount}",
            auctionId, bidResponse.Amount);
    }

    public async Task NotifyAuctionClosedAsync(int auctionId, int? winnerId, decimal finalPrice)
    {
        var groupName = AuctionHub.GetAuctionGroup(auctionId);

        await _hubContext.Clients.Group(groupName).SendAsync(HubMessages.AuctionClosed, new
        {
            auctionId,
            winnerId,
            finalPrice,
            closedAt = DateTime.UtcNow
        });

        _logger.LogInformation(
            "Sent AuctionClosed notification for auction {AuctionId}. Winner: {WinnerId}, Final price: {FinalPrice}",
            auctionId, winnerId, finalPrice);
    }

    public async Task NotifyTimeExtendedAsync(int auctionId, DateTime newEndTime)
    {
        var groupName = AuctionHub.GetAuctionGroup(auctionId);

        await _hubContext.Clients.Group(groupName).SendAsync(HubMessages.TimeExtended, new
        {
            auctionId,
            newEndTime,
            extendedAt = DateTime.UtcNow
        });

        _logger.LogDebug(
            "Sent TimeExtended notification for auction {AuctionId}. New end time: {NewEndTime}",
            auctionId, newEndTime);
    }

    public async Task NotifyAuctionEndingSoonAsync(int auctionId, int minutesRemaining)
    {
        var groupName = AuctionHub.GetAuctionGroup(auctionId);

        await _hubContext.Clients.Group(groupName).SendAsync(HubMessages.AuctionEndingSoon, new
        {
            auctionId,
            minutesRemaining,
            timestamp = DateTime.UtcNow
        });

        _logger.LogDebug(
            "Sent AuctionEndingSoon notification for auction {AuctionId}. Minutes remaining: {Minutes}",
            auctionId, minutesRemaining);
    }

    public async Task SendUserNotificationAsync(int userId, string type, string title, string message)
    {
        var groupName = AuctionHub.GetUserGroup(userId);

        await _hubContext.Clients.Group(groupName).SendAsync(HubMessages.UserNotification, new
        {
            type,
            title,
            message,
            timestamp = DateTime.UtcNow
        });

        _logger.LogDebug(
            "Sent personal notification to user {UserId}: {Type} - {Title}",
            userId, type, title);
    }

    public async Task NotifyAuctionsClosedBatchAsync(int count)
    {
        await _hubContext.Clients.All.SendAsync(HubMessages.AuctionsClosed, new
        {
            count,
            timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("Sent batch auction closed notification: {Count} auctions closed", count);
    }
}
