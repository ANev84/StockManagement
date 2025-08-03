using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace StockApi.StockDataService.Cache
{
    public class CacheServiceFactory
    {
        private readonly IServiceProvider _provider;
        private readonly CacheSettings _settings;

        public CacheServiceFactory(IServiceProvider provider, IOptions<CacheSettings> settings)
        {
            _provider = provider;
            _settings = settings.Value;
        }

        public async Task<IStockCacheService> CreateAsync()
        {
            var logger = _provider.GetRequiredService<ILogger<CacheServiceFactory>>();

            if (_settings.Type.Equals("Redis", StringComparison.OrdinalIgnoreCase))
            {
                var redis = _provider.GetRequiredService<IDistributedCache>();

                // Optional: try a ping test if needed (but no try/catch fallback)
                try
                {
                    await redis.SetStringAsync("ping:test", "1", new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Redis selected but unavailable.");
                    throw; // or return fallback, depending on design
                }

                return new RedisStockCacheService(redis);
            }

            logger.LogInformation("Using InMemory cache as configured.");
            return new InMemoryStockCacheService();
        }
    }
}
