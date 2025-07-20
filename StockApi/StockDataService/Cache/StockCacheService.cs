using Microsoft.Extensions.Caching.Distributed;
using StockApi.Models;
using StockApi.StockDataService.Cache;
using System.Text.Json;

public class StockCacheService : IStockCacheService
{
    private readonly IDistributedCache _cache;
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    private static readonly DistributedCacheEntryOptions _distributedCacheEntryOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) };

    public StockCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<StockData?> GetStockAsync(string ticker)
    {
        var json = await _cache.GetStringAsync($"stock:{ticker}:details");
        return json == null ? null : JsonSerializer.Deserialize<StockData>(json, _jsonOptions);
    }

    public async Task SetStockAsync(string ticker, StockData data)
    {
        var json = JsonSerializer.Serialize(data);
        await _cache.SetStringAsync($"stock:{ticker}:details", json, _distributedCacheEntryOptions);
    }

    public async Task<List<string>?> GetAllTickersAsync()
    {
        var json = await _cache.GetStringAsync("stock:tickers");
        return json == null ? null : JsonSerializer.Deserialize<List<string>>(json, _jsonOptions);
    }

    public async Task SetAllTickersAsync(List<string> tickers)
    {
        var json = JsonSerializer.Serialize(tickers);
        await _cache.SetStringAsync("stock:tickers", json, _distributedCacheEntryOptions);
    }
}
