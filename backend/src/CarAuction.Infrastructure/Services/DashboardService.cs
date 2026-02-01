using CarAuction.Application.Interfaces;
using CarAuction.Domain.Enums;
using CarAuction.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarAuction.Infrastructure.Services;

/// <summary>
/// Service for admin dashboard statistics with caching
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public DashboardService(ApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        // Try to get from cache first (cache for 5 minutes)
        return await _cacheService.GetOrCreateAsync(
            CacheKeys.DashboardStats,
            async () => await ComputeDashboardStatsAsync(),
            CacheDurations.Medium);
    }

    private async Task<DashboardStats> ComputeDashboardStatsAsync()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var sixMonthsAgo = now.AddMonths(-6);

        var stats = new DashboardStats
        {
            TotalUsers = await _context.Users.CountAsync(),
            ActiveUsers = await _context.Users.CountAsync(u => u.Status == UserStatus.Active),
            TotalCars = await _context.Cars.CountAsync(),
            ActiveAuctions = await _context.Auctions.CountAsync(a => a.Status == AuctionStatus.Active),
            CompletedAuctions = await _context.Auctions.CountAsync(a => a.Status == AuctionStatus.Completed),
            TotalRevenue = await _context.AuctionHistories
                .Where(h => h.ReserveMet && h.FinalPrice.HasValue)
                .SumAsync(h => h.FinalPrice ?? 0),
            TotalBids = await _context.Bids.CountAsync(),
            NewUsersThisMonth = await _context.Users
                .CountAsync(u => u.CreatedAt >= startOfMonth)
        };

        // Monthly auctions for last 6 months
        stats.MonthlyAuctions = await _context.AuctionHistories
            .Where(h => h.CompletedAt >= sixMonthsAgo)
            .GroupBy(h => new { h.CompletedAt!.Value.Year, h.CompletedAt.Value.Month })
            .Select(g => new MonthlyStats
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Label = $"{g.Key.Month}/{g.Key.Year}",
                Value = g.Count()
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToListAsync();

        // Monthly revenue for last 6 months
        stats.MonthlyRevenue = await _context.AuctionHistories
            .Where(h => h.CompletedAt >= sixMonthsAgo && h.ReserveMet && h.FinalPrice.HasValue)
            .GroupBy(h => new { h.CompletedAt!.Value.Year, h.CompletedAt.Value.Month })
            .Select(g => new MonthlyStats
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Label = $"{g.Key.Month}/{g.Key.Year}",
                Value = g.Sum(h => h.FinalPrice ?? 0)
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToListAsync();

        return stats;
    }
}
