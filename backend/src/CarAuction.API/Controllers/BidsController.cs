using CarAuction.Application.DTOs.Bid;
using CarAuction.Application.DTOs.Common;
using CarAuction.Application.Interfaces;
using CarAuction.API.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CarAuction.API.Controllers;

[ApiController]
[Route("api/v1/bids")]
[Authorize]
public class BidsController : ControllerBase
{
    private readonly IBidService _bidService;
    private readonly IHubContext<AuctionHub> _hubContext;

    public BidsController(IBidService bidService, IHubContext<AuctionHub> hubContext)
    {
        _bidService = bidService;
        _hubContext = hubContext;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BidResponse>>> PlaceBid([FromBody] CreateBidRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _bidService.PlaceBidAsync(userId, request, ipAddress);

        // Notify all clients watching this auction
        await _hubContext.Clients.Group($"auction_{request.AuctionId}")
            .SendAsync("BidPlaced", new
            {
                auctionId = request.AuctionId,
                currentBid = result.NewCurrentBid,
                totalBids = result.TotalBids,
                newEndTime = result.NewEndTime,
                timeExtended = result.TimeExtended
            });

        return Ok(ApiResponse<BidResponse>.SuccessResponse(result, "Puja realizada exitosamente"));
    }
}
