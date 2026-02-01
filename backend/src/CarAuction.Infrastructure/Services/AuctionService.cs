using AutoMapper;
using CarAuction.Application.DTOs.Auction;
using CarAuction.Application.DTOs.Bid;
using CarAuction.Application.DTOs.Common;
using CarAuction.Application.Interfaces;
using CarAuction.Domain.Entities;
using CarAuction.Domain.Enums;
using CarAuction.Domain.Exceptions;
using CarAuction.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarAuction.Infrastructure.Services;

public class AuctionService : IAuctionService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public AuctionService(
        ApplicationDbContext context,
        IMapper mapper,
        INotificationService notificationService)
    {
        _context = context;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<AuctionDto> GetByIdAsync(int id)
    {
        var auction = await _context.Auctions
            .Include(a => a.Car)
            .ThenInclude(c => c.Images.OrderBy(i => i.DisplayOrder))
            .Include(a => a.CurrentBidder)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (auction == null)
        {
            throw new NotFoundException(nameof(Auction), id);
        }

        return _mapper.Map<AuctionDto>(auction);
    }

    public async Task<PaginatedResult<AuctionListDto>> GetAllAsync(AuctionFilterRequest request)
    {
        var query = _context.Auctions
            .Include(a => a.Car)
            .ThenInclude(c => c.Images)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<AuctionStatus>(request.Status, true, out var status))
            {
                query = query.Where(a => a.Status == status);
            }
        }

        if (!string.IsNullOrEmpty(request.Brand))
            query = query.Where(a => a.Car.Brand.Contains(request.Brand));

        if (request.MinPrice.HasValue)
            query = query.Where(a => a.CurrentBid >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(a => a.CurrentBid <= request.MaxPrice.Value);

        if (request.EndingSoon == true)
            query = query.Where(a => a.Status == AuctionStatus.Active && a.EndTime <= DateTime.UtcNow.AddHours(1));

        var totalItems = await query.CountAsync();

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "price" => request.SortDescending ? query.OrderByDescending(a => a.CurrentBid) : query.OrderBy(a => a.CurrentBid),
            "endtime" => request.SortDescending ? query.OrderByDescending(a => a.EndTime) : query.OrderBy(a => a.EndTime),
            "bids" => request.SortDescending ? query.OrderByDescending(a => a.TotalBids) : query.OrderBy(a => a.TotalBids),
            _ => query.OrderBy(a => a.EndTime)
        };

        var auctions = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PaginatedResult<AuctionListDto>
        {
            Items = _mapper.Map<IEnumerable<AuctionListDto>>(auctions),
            TotalItems = totalItems,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<PaginatedResult<AuctionListDto>> GetActiveAuctionsAsync(AuctionFilterRequest request)
    {
        request.Status = "Active";
        return await GetAllAsync(request);
    }

    public async Task<IEnumerable<BidDto>> GetBidsAsync(int auctionId)
    {
        var auction = await _context.Auctions.FindAsync(auctionId);
        if (auction == null)
        {
            throw new NotFoundException(nameof(Auction), auctionId);
        }

        var bids = await _context.Bids
            .Include(b => b.User)
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.Amount)
            .ToListAsync();

        return _mapper.Map<IEnumerable<BidDto>>(bids);
    }

    public async Task<AuctionDto> CreateAsync(CreateAuctionRequest request)
    {
        var car = await _context.Cars
            .Include(c => c.Auction)
            .FirstOrDefaultAsync(c => c.Id == request.CarId);

        if (car == null)
        {
            throw new NotFoundException(nameof(Car), request.CarId);
        }

        if (car.Auction != null && car.Auction.Status == AuctionStatus.Active)
        {
            throw new BadRequestException("El carro ya tiene una subasta activa");
        }

        var auction = _mapper.Map<Auction>(request);
        auction.CurrentBid = request.StartingPrice;
        auction.OriginalEndTime = request.EndTime;
        auction.Status = request.StartTime <= DateTime.UtcNow ? AuctionStatus.Active : AuctionStatus.Pending;

        _context.Auctions.Add(auction);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(auction.Id);
    }

    public async Task<AuctionDto> UpdateAsync(int id, UpdateAuctionRequest request)
    {
        var auction = await _context.Auctions.FindAsync(id);
        if (auction == null)
        {
            throw new NotFoundException(nameof(Auction), id);
        }

        if (auction.Status == AuctionStatus.Active && auction.TotalBids > 0)
        {
            throw new BadRequestException("No se puede modificar una subasta activa con pujas");
        }

        if (request.StartingPrice.HasValue) auction.StartingPrice = request.StartingPrice.Value;
        if (request.ReservePrice.HasValue) auction.ReservePrice = request.ReservePrice.Value;
        if (request.MinimumBidIncrement.HasValue) auction.MinimumBidIncrement = request.MinimumBidIncrement.Value;
        if (request.StartTime.HasValue) auction.StartTime = request.StartTime.Value;
        if (request.EndTime.HasValue)
        {
            auction.EndTime = request.EndTime.Value;
            auction.OriginalEndTime = request.EndTime.Value;
        }
        if (request.ExtensionMinutes.HasValue) auction.ExtensionMinutes = request.ExtensionMinutes.Value;
        if (request.ExtensionThresholdMinutes.HasValue) auction.ExtensionThresholdMinutes = request.ExtensionThresholdMinutes.Value;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task CancelAuctionAsync(int id)
    {
        var auction = await _context.Auctions.FindAsync(id);
        if (auction == null)
        {
            throw new NotFoundException(nameof(Auction), id);
        }

        if (auction.Status == AuctionStatus.Completed)
        {
            throw new BadRequestException("No se puede cancelar una subasta completada");
        }

        auction.Status = AuctionStatus.Cancelled;
        await _context.SaveChangesAsync();

        // Notify bidders
        var bidders = await _context.Bids
            .Where(b => b.AuctionId == id)
            .Select(b => b.UserId)
            .Distinct()
            .ToListAsync();

        foreach (var bidderId in bidders)
        {
            await _notificationService.CreateNotificationAsync(
                bidderId,
                NotificationType.AuctionCancelled,
                "Subasta cancelada",
                $"La subasta en la que participaste ha sido cancelada",
                id);
        }
    }

    public async Task<int> CloseExpiredAuctionsAsync()
    {
        var expiredAuctions = await _context.Auctions
            .Include(a => a.Car)
            .Include(a => a.CurrentBidder)
            .Where(a => a.Status == AuctionStatus.Active && a.EndTime <= DateTime.UtcNow)
            .ToListAsync();

        var closedCount = 0;

        foreach (var auction in expiredAuctions)
        {
            auction.Status = AuctionStatus.Completed;

            var reserveMet = !auction.ReservePrice.HasValue || auction.CurrentBid >= auction.ReservePrice.Value;

            var history = new AuctionHistory
            {
                AuctionId = auction.Id,
                WinnerId = reserveMet ? auction.CurrentBidderId : null,
                FinalPrice = auction.CurrentBid,
                TotalBids = auction.TotalBids,
                UniqueParticipants = await _context.Bids.Where(b => b.AuctionId == auction.Id).Select(b => b.UserId).Distinct().CountAsync(),
                CompletedAt = DateTime.UtcNow,
                ReserveMet = reserveMet
            };

            _context.AuctionHistories.Add(history);

            if (auction.CurrentBidderId.HasValue && reserveMet)
            {
                var winningBid = await _context.Bids
                    .FirstOrDefaultAsync(b => b.AuctionId == auction.Id && b.UserId == auction.CurrentBidderId);
                if (winningBid != null)
                {
                    winningBid.IsWinningBid = true;
                }

                await _notificationService.NotifyAuctionWonAsync(
                    auction.CurrentBidderId.Value,
                    auction.Id,
                    auction.CurrentBid);
            }

            closedCount++;
        }

        await _context.SaveChangesAsync();
        return closedCount;
    }
}
