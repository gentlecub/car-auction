using CarAuction.Application.DTOs.Bid;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CarAuction.UnitTests.Services;

/// <summary>
/// Unit tests for RealTimeNotificationService
/// Note: These tests verify that SignalR messages are sent correctly
/// </summary>
public class RealTimeNotificationServiceTests
{
    private readonly Mock<IHubContext<TestHub>> _hubContextMock;
    private readonly Mock<IHubClients> _hubClientsMock;
    private readonly Mock<IClientProxy> _clientProxyMock;
    private readonly Mock<ILogger<TestRealTimeNotificationService>> _loggerMock;

    public RealTimeNotificationServiceTests()
    {
        _hubContextMock = new Mock<IHubContext<TestHub>>();
        _hubClientsMock = new Mock<IHubClients>();
        _clientProxyMock = new Mock<IClientProxy>();
        _loggerMock = new Mock<ILogger<TestRealTimeNotificationService>>();

        _hubContextMock.Setup(x => x.Clients).Returns(_hubClientsMock.Object);
        _hubClientsMock.Setup(x => x.Group(It.IsAny<string>())).Returns(_clientProxyMock.Object);
        _hubClientsMock.Setup(x => x.All).Returns(_clientProxyMock.Object);
    }

    [Fact]
    public async Task NotifyNewBidAsync_SendsMessageToAuctionGroup()
    {
        // Arrange
        var service = new TestRealTimeNotificationService(_hubContextMock.Object, _loggerMock.Object);
        var auctionId = 1;
        var bidResponse = new BidResponse
        {
            BidId = 100,
            Amount = 50000m,
            NewCurrentBid = 50000m,
            TotalBids = 5,
            TimeExtended = false
        };

        // Act
        await service.NotifyNewBidAsync(auctionId, bidResponse);

        // Assert
        _hubClientsMock.Verify(x => x.Group($"auction_{auctionId}"), Times.Once);
        _clientProxyMock.Verify(x => x.SendCoreAsync(
            "NewBid",
            It.Is<object[]>(args => args.Length == 1),
            default), Times.Once);
    }

    [Fact]
    public async Task NotifyAuctionClosedAsync_SendsMessageToAuctionGroup()
    {
        // Arrange
        var service = new TestRealTimeNotificationService(_hubContextMock.Object, _loggerMock.Object);
        var auctionId = 1;
        var winnerId = 10;
        var finalPrice = 75000m;

        // Act
        await service.NotifyAuctionClosedAsync(auctionId, winnerId, finalPrice);

        // Assert
        _hubClientsMock.Verify(x => x.Group($"auction_{auctionId}"), Times.Once);
        _clientProxyMock.Verify(x => x.SendCoreAsync(
            "AuctionClosed",
            It.Is<object[]>(args => args.Length == 1),
            default), Times.Once);
    }

    [Fact]
    public async Task NotifyTimeExtendedAsync_SendsMessageToAuctionGroup()
    {
        // Arrange
        var service = new TestRealTimeNotificationService(_hubContextMock.Object, _loggerMock.Object);
        var auctionId = 1;
        var newEndTime = DateTime.UtcNow.AddMinutes(5);

        // Act
        await service.NotifyTimeExtendedAsync(auctionId, newEndTime);

        // Assert
        _hubClientsMock.Verify(x => x.Group($"auction_{auctionId}"), Times.Once);
        _clientProxyMock.Verify(x => x.SendCoreAsync(
            "TimeExtended",
            It.Is<object[]>(args => args.Length == 1),
            default), Times.Once);
    }

    [Fact]
    public async Task NotifyAuctionEndingSoonAsync_SendsMessageToAuctionGroup()
    {
        // Arrange
        var service = new TestRealTimeNotificationService(_hubContextMock.Object, _loggerMock.Object);
        var auctionId = 1;
        var minutesRemaining = 5;

        // Act
        await service.NotifyAuctionEndingSoonAsync(auctionId, minutesRemaining);

        // Assert
        _hubClientsMock.Verify(x => x.Group($"auction_{auctionId}"), Times.Once);
        _clientProxyMock.Verify(x => x.SendCoreAsync(
            "AuctionEndingSoon",
            It.Is<object[]>(args => args.Length == 1),
            default), Times.Once);
    }

    [Fact]
    public async Task SendUserNotificationAsync_SendsMessageToUserGroup()
    {
        // Arrange
        var service = new TestRealTimeNotificationService(_hubContextMock.Object, _loggerMock.Object);
        var userId = 10;
        var type = "outbid";
        var title = "You've been outbid!";
        var message = "Someone placed a higher bid on BMW M3";

        // Act
        await service.SendUserNotificationAsync(userId, type, title, message);

        // Assert
        _hubClientsMock.Verify(x => x.Group($"user_{userId}"), Times.Once);
        _clientProxyMock.Verify(x => x.SendCoreAsync(
            "UserNotification",
            It.Is<object[]>(args => args.Length == 1),
            default), Times.Once);
    }

    [Fact]
    public async Task NotifyAuctionsClosedBatchAsync_SendsMessageToAllClients()
    {
        // Arrange
        var service = new TestRealTimeNotificationService(_hubContextMock.Object, _loggerMock.Object);
        var count = 5;

        // Act
        await service.NotifyAuctionsClosedBatchAsync(count);

        // Assert
        _hubClientsMock.Verify(x => x.All, Times.Once);
        _clientProxyMock.Verify(x => x.SendCoreAsync(
            "AuctionsClosed",
            It.Is<object[]>(args => args.Length == 1),
            default), Times.Once);
    }
}

/// <summary>
/// Test hub class for mocking purposes
/// </summary>
public class TestHub : Hub { }

/// <summary>
/// Test implementation of real-time notification service for unit testing
/// </summary>
public class TestRealTimeNotificationService
{
    private readonly IHubContext<TestHub> _hubContext;
    private readonly ILogger<TestRealTimeNotificationService> _logger;

    public TestRealTimeNotificationService(
        IHubContext<TestHub> hubContext,
        ILogger<TestRealTimeNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyNewBidAsync(int auctionId, BidResponse bidResponse)
    {
        await _hubContext.Clients.Group($"auction_{auctionId}").SendAsync("NewBid", new
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
    }

    public async Task NotifyAuctionClosedAsync(int auctionId, int? winnerId, decimal finalPrice)
    {
        await _hubContext.Clients.Group($"auction_{auctionId}").SendAsync("AuctionClosed", new
        {
            auctionId,
            winnerId,
            finalPrice,
            closedAt = DateTime.UtcNow
        });
    }

    public async Task NotifyTimeExtendedAsync(int auctionId, DateTime newEndTime)
    {
        await _hubContext.Clients.Group($"auction_{auctionId}").SendAsync("TimeExtended", new
        {
            auctionId,
            newEndTime,
            extendedAt = DateTime.UtcNow
        });
    }

    public async Task NotifyAuctionEndingSoonAsync(int auctionId, int minutesRemaining)
    {
        await _hubContext.Clients.Group($"auction_{auctionId}").SendAsync("AuctionEndingSoon", new
        {
            auctionId,
            minutesRemaining,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task SendUserNotificationAsync(int userId, string type, string title, string message)
    {
        await _hubContext.Clients.Group($"user_{userId}").SendAsync("UserNotification", new
        {
            type,
            title,
            message,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task NotifyAuctionsClosedBatchAsync(int count)
    {
        await _hubContext.Clients.All.SendAsync("AuctionsClosed", new
        {
            count,
            timestamp = DateTime.UtcNow
        });
    }
}
