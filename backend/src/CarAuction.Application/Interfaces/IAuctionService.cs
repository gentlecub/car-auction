using CarAuction.Application.DTOs.Auction;
using CarAuction.Application.DTOs.Bid;
using CarAuction.Application.DTOs.Common;

namespace CarAuction.Application.Interfaces;

public interface IAuctionService
{
    Task<AuctionDto> GetByIdAsync(int id);
    Task<PaginatedResult<AuctionListDto>> GetAllAsync(AuctionFilterRequest request);
    Task<PaginatedResult<AuctionListDto>> GetActiveAuctionsAsync(AuctionFilterRequest request);
    Task<IEnumerable<BidDto>> GetBidsAsync(int auctionId);
    Task<AuctionDto> CreateAsync(CreateAuctionRequest request);
    Task<AuctionDto> UpdateAsync(int id, UpdateAuctionRequest request);
    Task CancelAuctionAsync(int id);
    Task<int> CloseExpiredAuctionsAsync();
}
