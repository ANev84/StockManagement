using StockApi.Models;

namespace StockApi.StockDataService
{
    public interface IStockDataService
    {
        List<StockData> GetAll();
    }
}
