namespace CarAuction.Application.Interfaces;

/// <summary>
/// Service for caching data (Redis or in-memory)
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get value from cache
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Set value in cache with optional expiration
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Remove value from cache
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove all values matching a pattern (e.g., "auctions:*")
    /// </summary>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if key exists in cache
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get or create - returns cached value or creates and caches new value
    /// </summary>
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class;
}

/// <summary>
/// Cache key constants
/// </summary>
public static class CacheKeys
{
    public const string AuctionPrefix = "auction:";
    public const string AuctionListPrefix = "auctions:list:";
    public const string CarPrefix = "car:";
    public const string UserPrefix = "user:";
    public const string DashboardStats = "dashboard:stats";

    public static string Auction(int id) => $"{AuctionPrefix}{id}";
    public static string Car(int id) => $"{CarPrefix}{id}";
    public static string User(int id) => $"{UserPrefix}{id}";
    public static string AuctionList(string filterHash) => $"{AuctionListPrefix}{filterHash}";
}

/// <summary>
/// Cache duration constants
/// </summary>
public static class CacheDurations
{
    public static readonly TimeSpan Short = TimeSpan.FromMinutes(1);
    public static readonly TimeSpan Medium = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan Long = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan VeryLong = TimeSpan.FromHours(1);
}
