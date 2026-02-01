using CarAuction.Application.DTOs.Common;
using CarAuction.Application.DTOs.Notification;
using CarAuction.Application.Interfaces;
using CarAuction.Domain.Entities;
using CarAuction.Domain.Enums;
using CarAuction.Domain.Exceptions;
using CarAuction.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarAuction.Infrastructure.Services;

/// <summary>
/// Service for managing user notifications
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public NotificationService(ApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task CreateNotificationAsync(int userId, NotificationType type, string title, string message, int? auctionId = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            AuctionId = auctionId
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task NotifyOutbidAsync(int previousBidderId, int auctionId, decimal newBidAmount)
    {
        var auction = await _context.Auctions
            .Include(a => a.Car)
            .FirstOrDefaultAsync(a => a.Id == auctionId);

        if (auction == null) return;

        var carName = $"{auction.Car.Brand} {auction.Car.Model} {auction.Car.Year}";

        await CreateNotificationAsync(
            previousBidderId,
            NotificationType.Outbid,
            "Has sido superado",
            $"Alguien ha superado tu puja en {carName}. Nueva puja: {newBidAmount:C}",
            auctionId);

        var user = await _context.Users.FindAsync(previousBidderId);
        if (user != null)
        {
            await _emailService.SendOutbidNotificationAsync(user.Email, carName, newBidAmount);
        }
    }

    public async Task NotifyAuctionWonAsync(int winnerId, int auctionId, decimal finalPrice)
    {
        var auction = await _context.Auctions
            .Include(a => a.Car)
            .FirstOrDefaultAsync(a => a.Id == auctionId);

        if (auction == null) return;

        var carName = $"{auction.Car.Brand} {auction.Car.Model} {auction.Car.Year}";

        await CreateNotificationAsync(
            winnerId,
            NotificationType.AuctionWon,
            "¡Felicidades! Ganaste la subasta",
            $"Has ganado la subasta de {carName} por {finalPrice:C}",
            auctionId);

        var user = await _context.Users.FindAsync(winnerId);
        if (user != null)
        {
            await _emailService.SendAuctionWonAsync(user.Email, carName, finalPrice);
        }
    }

    public async Task NotifyAuctionEndingSoonAsync(int auctionId)
    {
        var bidders = await _context.Bids
            .Where(b => b.AuctionId == auctionId)
            .Select(b => b.UserId)
            .Distinct()
            .ToListAsync();

        var auction = await _context.Auctions
            .Include(a => a.Car)
            .FirstOrDefaultAsync(a => a.Id == auctionId);

        if (auction == null) return;

        var carName = $"{auction.Car.Brand} {auction.Car.Model} {auction.Car.Year}";

        foreach (var bidderId in bidders)
        {
            await CreateNotificationAsync(
                bidderId,
                NotificationType.AuctionEnding,
                "Subasta por terminar",
                $"La subasta de {carName} termina pronto",
                auctionId);
        }
    }

    public async Task<PaginatedResult<NotificationDto>> GetUserNotificationsAsync(int userId, PaginationRequest request)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt);

        var totalItems = await query.CountAsync();

        var notifications = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type.ToString(),
                Title = n.Title,
                Message = n.Message,
                AuctionId = n.AuctionId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt
            })
            .ToListAsync();

        return new PaginatedResult<NotificationDto>
        {
            Items = notifications,
            TotalItems = totalItems,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<NotificationSummary> GetSummaryAsync(int userId)
    {
        var unreadCount = await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

        var totalCount = await _context.Notifications
            .CountAsync(n => n.UserId == userId);

        var recentNotifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(5)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type.ToString(),
                Title = n.Title,
                Message = n.Message,
                AuctionId = n.AuctionId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt
            })
            .ToListAsync();

        return new NotificationSummary
        {
            UnreadCount = unreadCount,
            TotalCount = totalCount,
            RecentNotifications = recentNotifications
        };
    }

    public async Task<NotificationDto?> GetByIdAsync(int userId, int notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null) return null;

        return new NotificationDto
        {
            Id = notification.Id,
            Type = notification.Type.ToString(),
            Title = notification.Title,
            Message = notification.Message,
            AuctionId = notification.AuctionId,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt
        };
    }

    public async Task MarkAsReadAsync(int userId, int notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task DeleteAsync(int userId, int notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null)
        {
            throw new NotFoundException("Notificación", notificationId);
        }

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteOldNotificationsAsync(int userId, int daysOld = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);

        var oldNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && n.IsRead && n.CreatedAt < cutoffDate)
            .ToListAsync();

        _context.Notifications.RemoveRange(oldNotifications);
        await _context.SaveChangesAsync();
    }
}
