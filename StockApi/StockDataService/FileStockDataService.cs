using StockApi.Models;
using StockApi.StockDataService;
using System.Text.Json;

public class FileStockDataService : IStockDataService
{
    private readonly string _filePath = Path.Combine("Data", "stocks.json");
    private readonly ILogger<FileStockDataService> _logger;

    public FileStockDataService(ILogger<FileStockDataService> logger)
    {
        _logger = logger;
    }

    public List<StockData> GetAll()
    {
        if (!File.Exists(_filePath))
        {
            _logger.LogWarning("File not found: {FilePath}", _filePath);
            return new List<StockData>();
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<List<StockData>>(json, options);

            if (result == null)
            {
                _logger.LogError("Failed to deserialize stock data from file: deserialized result is null.");
                return new List<StockData>();
            }

            return result;
        }
        catch (JsonException jex)
        {
            _logger.LogError(jex, "JSON format error while reading file {FilePath}", _filePath);
            return new List<StockData>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while reading file {FilePath}", _filePath);
            return new List<StockData>();
        }
    }
}
