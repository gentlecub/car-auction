using CarAuction.Application.DTOs.Auth;

namespace CarAuction.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress = null, string? userAgent = null);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, string? ipAddress = null, string? userAgent = null);
    Task<bool> VerifyEmailAsync(string token);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task RevokeTokenAsync(string refreshToken);
}
