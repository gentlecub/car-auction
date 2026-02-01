using CarAuction.Infrastructure.Services;
using CarAuction.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CarAuction.UnitTests.Services;

public class EmailServiceTests
{
    private readonly Mock<ILogger<EmailService>> _loggerMock;
    private readonly EmailSettings _settings;

    public EmailServiceTests()
    {
        _loggerMock = new Mock<ILogger<EmailService>>();
        _settings = new EmailSettings
        {
            SmtpHost = "smtp.test.com",
            SmtpPort = 587,
            SmtpUser = "testuser",
            SmtpPassword = "testpassword",
            FromEmail = "noreply@test.com",
            FromName = "CarAuction Test",
            UseSsl = true,
            EnableSending = false, // Disable actual sending for tests
            BaseUrl = "http://localhost:5173"
        };
    }

    private EmailService CreateService()
    {
        var options = Options.Create(_settings);
        return new EmailService(options, _loggerMock.Object);
    }

    [Fact]
    public async Task SendEmailVerificationAsync_WhenSendingDisabled_LogsMessage()
    {
        // Arrange
        var service = CreateService();
        var email = "test@example.com";
        var token = "verification-token-123";

        // Act
        await service.SendEmailVerificationAsync(email, token);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("[DEV MODE]")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetAsync_WhenSendingDisabled_LogsMessage()
    {
        // Arrange
        var service = CreateService();
        var email = "test@example.com";
        var token = "reset-token-456";

        // Act
        await service.SendPasswordResetAsync(email, token);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("[DEV MODE]")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAuctionWonAsync_WhenSendingDisabled_LogsMessage()
    {
        // Arrange
        var service = CreateService();
        var email = "winner@example.com";
        var carName = "2023 BMW M3";
        var amount = 75000.00m;

        // Act
        await service.SendAuctionWonAsync(email, carName, amount);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("[DEV MODE]")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendOutbidNotificationAsync_WhenSendingDisabled_LogsMessage()
    {
        // Arrange
        var service = CreateService();
        var email = "outbid@example.com";
        var carName = "2023 Mercedes C300";
        var newBid = 55000.00m;

        // Act
        await service.SendOutbidNotificationAsync(email, carName, newBid);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("[DEV MODE]")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendEmailVerificationAsync_BuildsCorrectUrl()
    {
        // Arrange
        var service = CreateService();
        var email = "test@example.com";
        var token = "my-verification-token";

        // Act
        await service.SendEmailVerificationAsync(email, token);

        // Assert - Check that the URL is correctly formed in logs
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("http://localhost:5173/verify-email?token=my-verification-token")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetAsync_BuildsCorrectUrl()
    {
        // Arrange
        var service = CreateService();
        var email = "test@example.com";
        var token = "my-reset-token";

        // Act
        await service.SendPasswordResetAsync(email, token);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("http://localhost:5173/reset-password?token=my-reset-token")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
