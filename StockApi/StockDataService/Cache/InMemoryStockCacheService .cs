using StockApi.Models;
using System.Collections.Concurrent;

namespace StockApi.StockDataService.Cache
{
    public class InMemoryStockCacheService : IStockCacheService
    {
        private readonly ConcurrentDictionary<string, StockData> _stockCache = new();
        private List<StockData>? _tickersCache;

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

        public Task<List<StockData>?> GetAllTickersAsync()
        {
            return Task.FromResult(_tickersCache);
        }

        public Task SetAllTickersAsync(List<StockData> tickers)
        {
            _tickersCache = tickers;
            return Task.CompletedTask;
        }
    }
}
