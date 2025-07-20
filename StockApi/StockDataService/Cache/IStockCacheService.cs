using StockApi.Models;

namespace StockApi.StockDataService.Cache
{
    public interface IStockCacheService
    {
        Task<StockData?> GetStockAsync(string ticker);
        Task SetStockAsync(string ticker, StockData data);
        Task<List<string>?> GetAllTickersAsync();
        Task SetAllTickersAsync(List<string> tickers);
    }
}
