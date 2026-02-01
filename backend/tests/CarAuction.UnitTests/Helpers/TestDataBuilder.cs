namespace CarAuction.UnitTests.Helpers;

/// <summary>
/// Builder class for creating test data entities
/// </summary>
public static class TestDataBuilder
{
    /// <summary>
    /// Creates a test user with default values
    /// </summary>
    public static User CreateUser(
        int id = 1,
        string email = "test@example.com",
        string firstName = "Test",
        string lastName = "User",
        UserStatus status = UserStatus.Active,
        bool emailVerified = true)
    {
        return new User
        {
            Id = id,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!", 12),
            FirstName = firstName,
            LastName = lastName,
            Status = status,
            EmailVerified = emailVerified,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test car with default values
    /// </summary>
    public static Car CreateCar(
        int id = 1,
        string brand = "Toyota",
        string model = "Corolla",
        int year = 2022,
        int mileage = 25000,
        bool isActive = true)
    {
        return new Car
        {
            Id = id,
            Brand = brand,
            Model = model,
            Year = year,
            Mileage = mileage,
            Color = "Blanco",
            EngineType = "2.0L",
            Transmission = "Automático",
            FuelType = "Gasolina",
            Horsepower = 169,
            Description = "Test car description",
            Condition = "Excelente",
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test auction with default values
    /// </summary>
    public static Auction CreateAuction(
        int id = 1,
        int carId = 1,
        decimal startingPrice = 10000m,
        decimal currentBid = 10000m,
        decimal minimumBidIncrement = 100m,
        AuctionStatus status = AuctionStatus.Active,
        int? currentBidderId = null,
        DateTime? endTime = null)
    {
        return new Auction
        {
            Id = id,
            CarId = carId,
            StartingPrice = startingPrice,
            CurrentBid = currentBid,
            MinimumBidIncrement = minimumBidIncrement,
            ReservePrice = startingPrice * 1.5m,
            StartTime = DateTime.UtcNow.AddDays(-1),
            EndTime = endTime ?? DateTime.UtcNow.AddDays(7),
            OriginalEndTime = endTime ?? DateTime.UtcNow.AddDays(7),
            ExtensionMinutes = 5,
            ExtensionThresholdMinutes = 2,
            TotalBids = 0,
            Status = status,
            CurrentBidderId = currentBidderId,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test bid with default values
    /// </summary>
    public static Bid CreateBid(
        int id = 1,
        int auctionId = 1,
        int userId = 1,
        decimal amount = 10500m,
        bool isWinningBid = false)
    {
        return new Bid
        {
            Id = id,
            AuctionId = auctionId,
            UserId = userId,
            Amount = amount,
            IsWinningBid = isWinningBid,
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test role with default values
    /// </summary>
    public static Role CreateRole(int id = 1, string name = "User")
    {
        return new Role
        {
            Id = id,
            Name = name,
            Description = $"Role {name}",
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test refresh token
    /// </summary>
    public static RefreshToken CreateRefreshToken(
        int id = 1,
        int userId = 1,
        bool isRevoked = false,
        DateTime? expiresAt = null)
    {
        return new RefreshToken
        {
            Id = id,
            UserId = userId,
            Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(7),
            IsRevoked = isRevoked,
            CreatedAt = DateTime.UtcNow
        };
    }
}
