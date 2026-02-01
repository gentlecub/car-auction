using CarAuction.Application.DTOs.Common;
using CarAuction.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.API.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public AdminController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<DashboardStats>>> GetDashboard()
    {
        var result = await _dashboardService.GetDashboardStatsAsync();
        return Ok(ApiResponse<DashboardStats>.SuccessResponse(result));
    }
}
