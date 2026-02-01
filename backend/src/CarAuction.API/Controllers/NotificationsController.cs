using CarAuction.Application.DTOs.Common;
using CarAuction.Application.DTOs.Notification;
using CarAuction.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarAuction.API.Controllers;

/// <summary>
/// Controller for managing user notifications
/// </summary>
[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Get current user's notifications with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<NotificationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResult<NotificationDto>>>> GetAll(
        [FromQuery] PaginationRequest request)
    {
        var userId = GetUserId();
        var result = await _notificationService.GetUserNotificationsAsync(userId, request);
        return Ok(ApiResponse<PaginatedResult<NotificationDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get notification summary (unread count and recent notifications)
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<NotificationSummary>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<NotificationSummary>>> GetSummary()
    {
        var userId = GetUserId();
        var result = await _notificationService.GetSummaryAsync(userId);
        return Ok(ApiResponse<NotificationSummary>.SuccessResponse(result));
    }

    /// <summary>
    /// Get unread notifications count
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        var userId = GetUserId();
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(ApiResponse<int>.SuccessResponse(count));
    }

    /// <summary>
    /// Get a specific notification by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> GetById(int id)
    {
        var userId = GetUserId();
        var notification = await _notificationService.GetByIdAsync(userId, id);

        if (notification == null)
        {
            return NotFound(ApiResponse.CreateFail("Notificación no encontrada"));
        }

        return Ok(ApiResponse<NotificationDto>.SuccessResponse(notification));
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    [HttpPost("{id}/read")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> MarkAsRead(int id)
    {
        var userId = GetUserId();
        await _notificationService.MarkAsReadAsync(userId, id);
        return Ok(ApiResponse.CreateSuccess("Notificación marcada como leída"));
    }

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    [HttpPost("read-all")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> MarkAllAsRead()
    {
        var userId = GetUserId();
        await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(ApiResponse.CreateSuccess("Todas las notificaciones marcadas como leídas"));
    }

    /// <summary>
    /// Delete a notification
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        var userId = GetUserId();
        await _notificationService.DeleteAsync(userId, id);
        return Ok(ApiResponse.CreateSuccess("Notificación eliminada"));
    }

    /// <summary>
    /// Delete all old read notifications (older than 30 days by default)
    /// </summary>
    [HttpDelete("cleanup")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> CleanupOldNotifications([FromQuery] int daysOld = 30)
    {
        var userId = GetUserId();
        await _notificationService.DeleteOldNotificationsAsync(userId, daysOld);
        return Ok(ApiResponse.CreateSuccess($"Notificaciones leídas con más de {daysOld} días eliminadas"));
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
