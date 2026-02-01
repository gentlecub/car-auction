using CarAuction.Application.DTOs.Auction;
using CarAuction.Application.DTOs.Bid;
using CarAuction.Application.DTOs.Common;
using CarAuction.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.API.Controllers;

[ApiController]
[Route("api/v1/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly IAuctionService _auctionService;

    public AuctionsController(IAuctionService auctionService)
    {
        _auctionService = auctionService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<AuctionListDto>>>> GetAll([FromQuery] AuctionFilterRequest request)
    {
        var result = await _auctionService.GetAllAsync(request);
        return Ok(ApiResponse<PaginatedResult<AuctionListDto>>.SuccessResponse(result));
    }

    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<AuctionListDto>>>> GetActive([FromQuery] AuctionFilterRequest request)
    {
        var result = await _auctionService.GetActiveAuctionsAsync(request);
        return Ok(ApiResponse<PaginatedResult<AuctionListDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AuctionDto>>> GetById(int id)
    {
        var result = await _auctionService.GetByIdAsync(id);
        return Ok(ApiResponse<AuctionDto>.SuccessResponse(result));
    }

    [HttpGet("{id}/bids")]
    public async Task<ActionResult<ApiResponse<IEnumerable<BidDto>>>> GetBids(int id)
    {
        var result = await _auctionService.GetBidsAsync(id);
        return Ok(ApiResponse<IEnumerable<BidDto>>.SuccessResponse(result));
    }
}

[ApiController]
[Route("api/v1/admin/auctions")]
[Authorize(Roles = "Admin")]
public class AdminAuctionsController : ControllerBase
{
    private readonly IAuctionService _auctionService;

    public AdminAuctionsController(IAuctionService auctionService)
    {
        _auctionService = auctionService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AuctionDto>>> Create([FromBody] CreateAuctionRequest request)
    {
        var result = await _auctionService.CreateAsync(request);
        return CreatedAtAction(nameof(AuctionsController.GetById), "Auctions", new { id = result.Id },
            ApiResponse<AuctionDto>.SuccessResponse(result, "Subasta creada exitosamente"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<AuctionDto>>> Update(int id, [FromBody] UpdateAuctionRequest request)
    {
        var result = await _auctionService.UpdateAsync(id, request);
        return Ok(ApiResponse<AuctionDto>.SuccessResponse(result, "Subasta actualizada exitosamente"));
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ApiResponse>> Cancel(int id)
    {
        await _auctionService.CancelAuctionAsync(id);
        return Ok(ApiResponse.CreateSuccess("Subasta cancelada exitosamente"));
    }
}
