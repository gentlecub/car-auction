using CarAuction.Application.DTOs.Bid;
using CarAuction.Application.DTOs.Common;

namespace CarAuction.Application.Interfaces;

public interface IBidService
{
    Task<BidResponse> PlaceBidAsync(int userId, CreateBidRequest request, string? ipAddress = null);
    Task<PaginatedResult<BidDto>> GetUserBidsAsync(int userId, PaginationRequest request);
    Task<BidDto?> GetWinningBidAsync(int auctionId);
}
