using System.Text.Json.Serialization;

namespace StockApi.Models
{
    public record StockData(
     string Ticker,
     DateTime Date,
     decimal Open,
     decimal Close,
     decimal High,
     decimal Low,
     long Volume
    );

}
