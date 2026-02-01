using CarAuction.Application.Interfaces;
using CarAuction.Infrastructure.Data;
using CarAuction.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CarAuction.API.BackgroundServices;

/// <summary>
/// Background service that periodically closes expired auctions
/// </summary>
public class AuctionCloseService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuctionCloseService> _logger;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _checkInterval;

    public AuctionCloseService(
        IServiceProvider serviceProvider,
        ILogger<AuctionCloseService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;

        var intervalSeconds = configuration.GetValue("Auction:CloseCheckIntervalSeconds", 60);
        _checkInterval = TimeSpan.FromSeconds(intervalSeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "AuctionCloseService started. Check interval: {Interval}s",
            _checkInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredAuctionsAsync(stoppingToken);
                await CheckAuctionsEndingSoonAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AuctionCloseService processing cycle");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("AuctionCloseService stopped");
    }

    /// <summary>
    /// Close all expired auctions and notify clients
    /// </summary>
    private async Task ProcessExpiredAuctionsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var auctionService = scope.ServiceProvider.GetRequiredService<IAuctionService>();
        var realTimeService = scope.ServiceProvider.GetRequiredService<IRealTimeNotificationService>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Get auctions that are about to close (for individual notifications)
        var expiringAuctions = await context.Auctions
            .Where(a => a.Status == AuctionStatus.Active && a.EndTime <= DateTime.UtcNow)
            .Select(a => new { a.Id, a.CurrentBidderId, a.CurrentBid })
            .ToListAsync(stoppingToken);

        if (expiringAuctions.Count == 0)
            return;

        // Close all expired auctions
        var closedCount = await auctionService.CloseExpiredAuctionsAsync();

        if (closedCount > 0)
        {
            _logger.LogInformation("Closed {Count} expired auctions", closedCount);

            // Send individual notifications for each closed auction
            foreach (var auction in expiringAuctions)
            {
                try
                {
                    await realTimeService.NotifyAuctionClosedAsync(
                        auction.Id,
                        auction.CurrentBidderId,
                        auction.CurrentBid);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to send close notification for auction {AuctionId}",
                        auction.Id);
                }
            }

            // Also send batch notification
            await realTimeService.NotifyAuctionsClosedBatchAsync(closedCount);
        }
    }

    /// <summary>
    /// Notify clients about auctions ending soon
    /// </summary>
    private async Task CheckAuctionsEndingSoonAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var realTimeService = scope.ServiceProvider.GetRequiredService<IRealTimeNotificationService>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Find auctions ending in the next 5 minutes
        var threshold = DateTime.UtcNow.AddMinutes(5);
        var endingSoonAuctions = await context.Auctions
            .Where(a => a.Status == AuctionStatus.Active
                && a.EndTime > DateTime.UtcNow
                && a.EndTime <= threshold)
            .Select(a => new { a.Id, a.EndTime })
            .ToListAsync(stoppingToken);

        foreach (var auction in endingSoonAuctions)
        {
            var minutesRemaining = (int)(auction.EndTime - DateTime.UtcNow).TotalMinutes + 1;

            try
            {
                // Send real-time notification
                await realTimeService.NotifyAuctionEndingSoonAsync(auction.Id, minutesRemaining);

                // Create persistent notifications for bidders
                await notificationService.NotifyAuctionEndingSoonAsync(auction.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to send ending soon notification for auction {AuctionId}",
                    auction.Id);
            }
        }
    }
}
