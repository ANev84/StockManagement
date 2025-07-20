using Microsoft.Extensions.Caching.Distributed;

namespace StockApi.StockDataService.Cache
{
    public class CacheServiceFactory
    {
        private readonly IServiceProvider _provider;

        public CacheServiceFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async Task<IStockCacheService> CreateAsync()
        {
            var logger = _provider.GetRequiredService<ILogger<CacheServiceFactory>>();
            var redis = _provider.GetRequiredService<IDistributedCache>();

            try
            {
                //  Redis availability check
                await redis.SetStringAsync("ping:test", "1", new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5) });
                return new StockCacheService(redis);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis unavailable. Falling back to InMemory cache.");
                return new InMemoryStockCacheService();
            }
        }
    }
}
