using CarAuction.Application.DTOs.Auth;
using CarAuction.Application.DTOs.Common;
using CarAuction.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Registro exitoso"));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();
        var result = await _authService.LoginAsync(request, ipAddress, userAgent);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Inicio de sesión exitoso"));
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, ipAddress, userAgent);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result));
    }

    [HttpGet("verify-email/{token}")]
    public async Task<ActionResult<ApiResponse>> VerifyEmail(string token)
    {
        var result = await _authService.VerifyEmailAsync(token);
        if (result)
        {
            return Ok(ApiResponse.CreateSuccess("Email verificado exitosamente"));
        }
        return BadRequest(ApiResponse.CreateFail("Token inválido o expirado"));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _authService.ForgotPasswordAsync(request.Email);
        return Ok(ApiResponse.CreateSuccess("Si el email existe, recibirás instrucciones para restablecer tu contraseña"));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _authService.ResetPasswordAsync(request);
        return Ok(ApiResponse.CreateSuccess("Contraseña restablecida exitosamente"));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse>> Logout([FromBody] RefreshTokenRequest request)
    {
        await _authService.RevokeTokenAsync(request.RefreshToken);
        return Ok(ApiResponse.CreateSuccess("Sesión cerrada exitosamente"));
    }
}
