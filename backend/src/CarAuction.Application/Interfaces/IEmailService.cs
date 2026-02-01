namespace CarAuction.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string email, string token);
    Task SendPasswordResetAsync(string email, string token);
    Task SendAuctionWonAsync(string email, string carName, decimal amount);
    Task SendOutbidNotificationAsync(string email, string carName, decimal newBid);
}
