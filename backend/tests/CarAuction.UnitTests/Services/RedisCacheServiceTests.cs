using CarAuction.Infrastructure.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace CarAuction.UnitTests.Services;

public class RedisCacheServiceTests
{
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<RedisCacheService>> _loggerMock;
    private readonly RedisCacheService _service;

    public RedisCacheServiceTests()
    {
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<RedisCacheService>>();
        _service = new RedisCacheService(_cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAsync_WhenKeyExists_ReturnsDeserializedValue()
    {
        // Arrange
        var key = "test:key";
        var testData = new TestData { Id = 1, Name = "Test" };
        var serialized = JsonSerializer.Serialize(testData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        _cacheMock.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(serialized));

        // Act
        var result = await _service.GetAsync<TestData>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetAsync_WhenKeyNotExists_ReturnsNull()
    {
        // Arrange
        var key = "nonexistent:key";

        _cacheMock.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _service.GetAsync<TestData>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_SetsValueInCache()
    {
        // Arrange
        var key = "test:key";
        var testData = new TestData { Id = 1, Name = "Test" };

        // Act
        await _service.SetAsync(key, testData, TimeSpan.FromMinutes(5));

        // Assert
        _cacheMock.Verify(x => x.SetAsync(
            key,
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(5)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_RemovesKeyFromCache()
    {
        // Arrange
        var key = "test:key";

        // Act
        await _service.RemoveAsync(key);

        // Assert
        _cacheMock.Verify(x => x.RemoveAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyExists_ReturnsTrue()
    {
        // Arrange
        var key = "existing:key";

        _cacheMock.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new byte[] { 1, 2, 3 });

        // Act
        var result = await _service.ExistsAsync(key);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyNotExists_ReturnsFalse()
    {
        // Arrange
        var key = "nonexistent:key";

        _cacheMock.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _service.ExistsAsync(key);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenCached_ReturnsCachedValue()
    {
        // Arrange
        var key = "test:key";
        var cachedData = new TestData { Id = 1, Name = "Cached" };
        var serialized = JsonSerializer.Serialize(cachedData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        _cacheMock.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(serialized));

        var factoryCalled = false;

        // Act
        var result = await _service.GetOrCreateAsync(key, async () =>
        {
            factoryCalled = true;
            return await Task.FromResult(new TestData { Id = 2, Name = "Fresh" });
        });

        // Assert
        result.Id.Should().Be(1);
        result.Name.Should().Be("Cached");
        factoryCalled.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenNotCached_CallsFactoryAndCaches()
    {
        // Arrange
        var key = "test:key";
        var freshData = new TestData { Id = 2, Name = "Fresh" };

        _cacheMock.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var factoryCalled = false;

        // Act
        var result = await _service.GetOrCreateAsync(key, async () =>
        {
            factoryCalled = true;
            return await Task.FromResult(freshData);
        }, TimeSpan.FromMinutes(5));

        // Assert
        result.Id.Should().Be(2);
        result.Name.Should().Be("Fresh");
        factoryCalled.Should().BeTrue();

        _cacheMock.Verify(x => x.SetAsync(
            key,
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAsync_WhenExceptionOccurs_ReturnsNullAndLogs()
    {
        // Arrange
        var key = "error:key";

        _cacheMock.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis connection error"));

        // Act
        var result = await _service.GetAsync<TestData>(key);

        // Assert
        result.Should().BeNull();
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
