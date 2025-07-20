using StockApi.Models;

namespace StockApi.StockDataService.Cache
{
    public class InMemoryStockCacheService : IStockCacheService
    {
        private readonly Dictionary<string, StockData> _stockCache = new();
        private List<string>? _tickersCache;

        public Task<StockData?> GetStockAsync(string ticker)
        {
            _stockCache.TryGetValue(ticker.ToUpperInvariant(), out var stock);
            return Task.FromResult(stock);
        }

        public Task SetStockAsync(string ticker, StockData data)
        {
            _stockCache[ticker.ToUpperInvariant()] = data;
            return Task.CompletedTask;
        }

        public Task<List<string>?> GetAllTickersAsync()
        {
            return Task.FromResult(_tickersCache);
        }

        public Task SetAllTickersAsync(List<string> tickers)
        {
            _tickersCache = tickers.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            return Task.CompletedTask;
        }
    }
}
