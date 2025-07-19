using System.Text.Json.Serialization;

namespace StockApi.Models
{
    public class StockData
    {
        [JsonPropertyName("ticker")]
        public required string Ticker { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("open")]
        public decimal Open { get; set; }

        [JsonPropertyName("close")]
        public decimal Close { get; set; }

        [JsonPropertyName("high")]
        public decimal High { get; set; }

        [JsonPropertyName("low")]
        public decimal Low { get; set; }

        [JsonPropertyName("volume")]
        public long Volume { get; set; }
    }
}
