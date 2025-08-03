using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StockApi.StockDataService.Cache;
using System;
using System.Threading.Tasks;
using Xunit;

public class CacheServiceFactoryTests
{
    [Fact]
    public async Task CreateAsync_ShouldReturnRedisCacheService_WhenTypeIsRedis_AndRedisIsAvailable()
    {
        // Arrange
        var redisMock = new Mock<IDistributedCache>();
        redisMock.Setup(r => r.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(redisMock.Object);
        services.AddLogging(); // adds ILogger<T>
        var serviceProvider = services.BuildServiceProvider();

        var settings = Options.Create(new CacheSettings { Type = "Redis" });
        var factory = new CacheServiceFactory(serviceProvider, settings);

        // Act
        var result = await factory.CreateAsync();

        // Assert
        Assert.IsType<RedisStockCacheService>(result);
    }


    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenRedisFails()
    {
        // Arrange
        var redisMock = new Mock<IDistributedCache>();
        redisMock.Setup(r => r.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis is down"));

        var loggerMock = new Mock<ILogger<CacheServiceFactory>>();

        var services = new ServiceCollection();
        services.AddSingleton(redisMock.Object);
        services.AddSingleton(typeof(ILogger<CacheServiceFactory>), loggerMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        var settings = Options.Create(new CacheSettings { Type = "Redis" });
        var factory = new CacheServiceFactory(serviceProvider, settings);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(factory.CreateAsync);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnInMemoryCacheService_WhenTypeIsNotRedis()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CacheServiceFactory>>();

        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<CacheServiceFactory>), loggerMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        var settings = Options.Create(new CacheSettings { Type = "Memory" });
        var factory = new CacheServiceFactory(serviceProvider, settings);

        // Act
        var result = await factory.CreateAsync();

        // Assert
        Assert.IsType<InMemoryStockCacheService>(result);
    }
}
