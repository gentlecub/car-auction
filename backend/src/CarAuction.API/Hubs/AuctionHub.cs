using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CarAuction.API.Hubs;

/// <summary>
/// SignalR Hub for real-time auction updates
/// </summary>
public class AuctionHub : Hub
{
    private readonly ILogger<AuctionHub> _logger;

    public AuctionHub(ILogger<AuctionHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Join an auction group to receive updates for that auction
    /// </summary>
    public async Task JoinAuction(int auctionId)
    {
        var groupName = GetAuctionGroup(auctionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogDebug("Client {ConnectionId} joined auction {AuctionId}", Context.ConnectionId, auctionId);
    }

    /// <summary>
    /// Leave an auction group
    /// </summary>
    public async Task LeaveAuction(int auctionId)
    {
        var groupName = GetAuctionGroup(auctionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogDebug("Client {ConnectionId} left auction {AuctionId}", Context.ConnectionId, auctionId);
    }

    /// <summary>
    /// Join multiple auctions at once (for home page, etc.)
    /// </summary>
    public async Task JoinAuctions(int[] auctionIds)
    {
        foreach (var auctionId in auctionIds)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetAuctionGroup(auctionId));
        }
        _logger.LogDebug("Client {ConnectionId} joined {Count} auctions", Context.ConnectionId, auctionIds.Length);
    }

    /// <summary>
    /// Leave multiple auctions at once
    /// </summary>
    public async Task LeaveAuctions(int[] auctionIds)
    {
        foreach (var auctionId in auctionIds)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetAuctionGroup(auctionId));
        }
    }

    /// <summary>
    /// Register for personal notifications (requires authentication)
    /// </summary>
    [Authorize]
    public async Task JoinUserNotifications()
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetUserGroup(userId.Value));
            _logger.LogDebug("User {UserId} registered for personal notifications", userId.Value);
        }
    }

    /// <summary>
    /// Unregister from personal notifications
    /// </summary>
    [Authorize]
    public async Task LeaveUserNotifications()
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetUserGroup(userId.Value));
        }
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogDebug("Client connected: {ConnectionId}", Context.ConnectionId);

        // Auto-join user notifications if authenticated
        var userId = GetUserId();
        if (userId.HasValue)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetUserGroup(userId.Value));
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            _logger.LogWarning(exception, "Client disconnected with error: {ConnectionId}", Context.ConnectionId);
        }
        else
        {
            _logger.LogDebug("Client disconnected: {ConnectionId}", Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    #region Helper Methods

    private int? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    public static string GetAuctionGroup(int auctionId) => $"auction_{auctionId}";
    public static string GetUserGroup(int userId) => $"user_{userId}";

    #endregion
}

/// <summary>
/// DTO classes for SignalR messages
/// </summary>
public static class HubMessages
{
    public const string NewBid = "NewBid";
    public const string AuctionClosed = "AuctionClosed";
    public const string TimeExtended = "TimeExtended";
    public const string AuctionEndingSoon = "AuctionEndingSoon";
    public const string UserNotification = "UserNotification";
    public const string AuctionsClosed = "AuctionsClosed";
}
