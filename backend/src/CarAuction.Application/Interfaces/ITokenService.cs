using CarAuction.Domain.Entities;

namespace CarAuction.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    RefreshToken GenerateRefreshToken(int userId, string? ipAddress = null, string? userAgent = null);
    int? ValidateAccessToken(string token);
}
