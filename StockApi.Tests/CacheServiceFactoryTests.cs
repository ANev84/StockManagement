using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using StockApi.StockDataService.Cache;

namespace StockApi.Tests
{
    public class CacheServiceFactoryTests
    {
        [Fact]
        public async Task CreateAsync_ShouldReturnInMemoryStockCacheService_WhenRedisFails()
        {
            // Arrange
            var mockRedis = new Mock<IDistributedCache>();
            mockRedis
                .Setup(r => r.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Redis is down"));

            var mockLogger = new Mock<ILogger<CacheServiceFactory>>();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(mockRedis.Object);
            serviceCollection.AddSingleton(mockLogger.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var factory = new CacheServiceFactory(serviceProvider);

            // Act
            var result = await factory.CreateAsync();

            // Assert
            Assert.IsType<InMemoryStockCacheService>(result);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnStockCacheService_WhenRedisIsAvailable()
        {
            // Arrange
            var mockRedis = new Mock<IDistributedCache>();
            mockRedis
                .Setup(r => r.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var mockLogger = new Mock<ILogger<CacheServiceFactory>>();

            var services = new ServiceCollection();
            services.AddSingleton(mockRedis.Object);
            services.AddSingleton(mockLogger.Object);
            var provider = services.BuildServiceProvider();

            var factory = new CacheServiceFactory(provider);

            // Act
            var result = await factory.CreateAsync();

            // Assert
            Assert.IsType<StockCacheService>(result);
        }
    }
}
