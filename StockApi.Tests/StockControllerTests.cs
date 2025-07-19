using Microsoft.AspNetCore.Mvc;
using Moq;
using StockApi.Controllers;
using StockApi.Models;
using StockApi.StockDataService;
using System.Text.Json;


public class StockControllerTests
{
    private List<StockData> LoadTestData()
    {
        var json = File.ReadAllText("test_stocks.json");
        var testData =  JsonSerializer.Deserialize<List<StockData>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        if (testData == null)
        {
            throw new InvalidOperationException("Test data file 'test_stocks.json' could not be loaded.");
        }
        return testData;
    }

    [Fact]
    public void GetAllTickers_ShouldReturnListOfTickers()
    {
        // Arrange
        var data = LoadTestData();
        var mockService = new Mock<IStockDataService>();
        mockService.Setup(s => s.GetAll()).Returns(data);

        var controller = new StockController(mockService.Object);

        // Act
        var result = controller.GetAllTickers() as OkObjectResult;

        // Assert
        var tickers = Assert.IsType<List<string>>(result?.Value);
        Assert.Contains("AAPL", tickers);
        Assert.Contains("MSFT", tickers);
        Assert.Contains("GOOGL", tickers);
        Assert.Equal(3, tickers.Count);
    }

    [Fact]
    public void GetTickerDetails_ShouldReturnCorrectDetailsForAAPL()
    {
        // Arrange
        var data = LoadTestData();
        var mockService = new Mock<IStockDataService>();
        mockService.Setup(s => s.GetAll()).Returns(data);

        var controller = new StockController(mockService.Object);

        // Act
        var result = controller.GetTickerDetails("AAPL") as OkObjectResult;

        // Assert
        var stockList = Assert.IsType<List<StockData>>(result?.Value);
        var stock = stockList.FirstOrDefault();

        Assert.NotNull(stock);
        Assert.Equal("AAPL", stock.Ticker);
        Assert.Equal(198.15m, stock.Open);
        Assert.Equal(202.30m, stock.Close);
    }

    [Fact]
    public void GetBuyingOption_ShouldReturnNumberOfSharesForBudget()
    {
        // Arrange
        var data = LoadTestData();
        var mockService = new Mock<IStockDataService>();
        mockService.Setup(s => s.GetAll()).Returns(data);

        var controller = new StockController(mockService.Object);

        // Act
        var result = controller.GetBuyingOption("AAPL", 1000) as OkObjectResult;

        // Assert
        Assert.NotNull(result);

        var json = JsonSerializer.Serialize(result.Value);
        var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

        Assert.Equal("AAPL", parsed["Ticker"].GetString());
        Assert.Equal(1000m, parsed["Budget"].GetDecimal());
        Assert.Equal(4, parsed["Shares"].GetInt32()); // 1000 / 202.30 = 4.94 => 4
    }
}