using CarAuction.Application.DTOs.Bid;
using CarAuction.Application.DTOs.Common;
using CarAuction.Application.DTOs.User;
using CarAuction.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarAuction.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IBidService _bidService;

    public UsersController(IUserService userService, IBidService bidService)
    {
        _userService = userService;
        _bidService = bidService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _userService.GetCurrentUserAsync(userId);
        return Ok(ApiResponse<UserDto>.SuccessResponse(result));
    }

    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateCurrentUser([FromBody] UpdateUserRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _userService.UpdateAsync(userId, request);
        return Ok(ApiResponse<UserDto>.SuccessResponse(result, "Perfil actualizado exitosamente"));
    }

    [HttpPost("me/change-password")]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _userService.ChangePasswordAsync(userId, request);
        return Ok(ApiResponse.CreateSuccess("Contraseña actualizada exitosamente"));
    }

    [HttpGet("me/bids")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<BidDto>>>> GetMyBids([FromQuery] PaginationRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _bidService.GetUserBidsAsync(userId, request);
        return Ok(ApiResponse<PaginatedResult<BidDto>>.SuccessResponse(result));
    }
}

[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IUserService _userService;

    public AdminUsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<UserDto>>>> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _userService.GetAllAsync(request);
        return Ok(ApiResponse<PaginatedResult<UserDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        return Ok(ApiResponse<UserDto>.SuccessResponse(result));
    }

    [HttpPost("{id}/activate")]
    public async Task<ActionResult<ApiResponse>> Activate(int id)
    {
        await _userService.ActivateUserAsync(id);
        return Ok(ApiResponse.CreateSuccess("Usuario activado exitosamente"));
    }

    [HttpPost("{id}/deactivate")]
    public async Task<ActionResult<ApiResponse>> Deactivate(int id)
    {
        await _userService.DeactivateUserAsync(id);
        return Ok(ApiResponse.CreateSuccess("Usuario desactivado exitosamente"));
    }
}
