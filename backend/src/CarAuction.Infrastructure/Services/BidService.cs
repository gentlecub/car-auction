using AutoMapper;
using CarAuction.Application.DTOs.Bid;
using CarAuction.Application.DTOs.Common;
using CarAuction.Application.Interfaces;
using CarAuction.Domain.Entities;
using CarAuction.Domain.Enums;
using CarAuction.Domain.Exceptions;
using CarAuction.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarAuction.Infrastructure.Services;

public class BidService : IBidService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly IRealTimeNotificationService _realTimeService;
    private readonly ILogger<BidService> _logger;

    public BidService(
        ApplicationDbContext context,
        IMapper mapper,
        INotificationService notificationService,
        IRealTimeNotificationService realTimeService,
        ILogger<BidService> logger)
    {
        _context = context;
        _mapper = mapper;
        _notificationService = notificationService;
        _realTimeService = realTimeService;
        _logger = logger;
    }

    public async Task<BidResponse> PlaceBidAsync(int userId, CreateBidRequest request, string? ipAddress = null)
    {
        // Use a transaction for optimistic concurrency
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var auction = await _context.Auctions
                .Include(a => a.Car)
                .FirstOrDefaultAsync(a => a.Id == request.AuctionId);

            if (auction == null)
            {
                throw new NotFoundException(nameof(Auction), request.AuctionId);
            }

            if (auction.Status != AuctionStatus.Active)
            {
                throw new BadRequestException("La subasta no está activa");
            }

            if (auction.EndTime <= DateTime.UtcNow)
            {
                throw new BadRequestException("La subasta ha terminado");
            }

            var minimumBid = auction.CurrentBid + auction.MinimumBidIncrement;
            if (request.Amount < minimumBid)
            {
                throw new BadRequestException($"El monto mínimo de puja es {minimumBid:C}");
            }

            if (auction.CurrentBidderId == userId)
            {
                throw new BadRequestException("Ya eres el postor actual");
            }

            var previousBidderId = auction.CurrentBidderId;

            // Create bid
            var bid = new Bid
            {
                AuctionId = request.AuctionId,
                UserId = userId,
                Amount = request.Amount,
                IpAddress = ipAddress
            };

            _context.Bids.Add(bid);

            // Update auction
            auction.CurrentBid = request.Amount;
            auction.CurrentBidderId = userId;
            auction.TotalBids++;

            // Check if we need to extend time
            var timeRemaining = auction.EndTime - DateTime.UtcNow;
            var timeExtended = false;
            DateTime? newEndTime = null;

            if (timeRemaining.TotalMinutes <= auction.ExtensionThresholdMinutes)
            {
                auction.EndTime = DateTime.UtcNow.AddMinutes(auction.ExtensionMinutes);
                timeExtended = true;
                newEndTime = auction.EndTime;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Notify previous bidder (non-blocking)
            if (previousBidderId.HasValue && previousBidderId.Value != userId)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _notificationService.NotifyOutbidAsync(
                            previousBidderId.Value,
                            auction.Id,
                            request.Amount);
                    }
                    catch { }
                });
            }

            var response = new BidResponse
            {
                BidId = bid.Id,
                Amount = bid.Amount,
                NewCurrentBid = auction.CurrentBid,
                TotalBids = auction.TotalBids,
                NewEndTime = newEndTime,
                TimeExtended = timeExtended
            };

            // Send real-time notification to all watchers (non-blocking)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _realTimeService.NotifyNewBidAsync(auction.Id, response);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send real-time bid notification for auction {AuctionId}", auction.Id);
                }
            });

            return response;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<PaginatedResult<BidDto>> GetUserBidsAsync(int userId, PaginationRequest request)
    {
        var query = _context.Bids
            .Include(b => b.Auction)
            .ThenInclude(a => a.Car)
            .Include(b => b.User)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt);

        var totalItems = await query.CountAsync();

        var bids = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PaginatedResult<BidDto>
        {
            Items = _mapper.Map<IEnumerable<BidDto>>(bids),
            TotalItems = totalItems,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<BidDto?> GetWinningBidAsync(int auctionId)
    {
        var bid = await _context.Bids
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.AuctionId == auctionId && b.IsWinningBid);

        return bid != null ? _mapper.Map<BidDto>(bid) : null;
    }
}
