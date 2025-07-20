using Microsoft.AspNetCore.Mvc;
using Moq;
using StockApi.Controllers;
using StockApi.Models;
using StockApi.StockDataService;
using StockApi.StockDataService.Cache;
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
    public async void GetAllTickers_ShouldReturnListOfTickers()
    {
        // Arrange
        var data = LoadTestData();

        var mockService = new Mock<IStockDataService>();
        mockService.Setup(s => s.GetAll()).Returns(data);

        // Mock the cache to return null, simulating no cached data
        var mockCache = new Mock<IStockCacheService>();
        mockCache.Setup(c => c.GetAllTickersAsync()).ReturnsAsync((List<string>?)null);        
        
        var controller = new StockController(mockService.Object, mockCache.Object);
       
        // Act
        var result = await controller.GetAllTickers() as OkObjectResult;

        // Assert
        var tickers = Assert.IsType<List<string>>(result?.Value);
        Assert.Contains("AAPL", tickers);
        Assert.Contains("MSFT", tickers);
        Assert.Contains("GOOGL", tickers);
        Assert.Equal(3, tickers.Count);
    }

    [Fact]
    public async void GetTickerDetails_ShouldReturnCorrectDetailsForAAPL()
    {
        // Arrange
        var data = LoadTestData();

        var mockService = new Mock<IStockDataService>();
        mockService.Setup(s => s.GetAll()).Returns(data);

        // Mock the cache to return null, simulating no cached data
        var mockCache = new Mock<IStockCacheService>();
        mockCache.Setup(s => s.GetStockAsync(It.IsAny<string>())).ReturnsAsync((StockData?)null); 

        var controller = new StockController(mockService.Object, mockCache.Object);

        // Act
        var result = await controller.GetTickerDetails("AAPL") as OkObjectResult;

        // Assert
        var stock = Assert.IsType<StockData>(result?.Value);
     
        Assert.NotNull(stock);
        Assert.Equal("AAPL", stock.Ticker);
        Assert.Equal(198.15m, stock.Open);
        Assert.Equal(202.30m, stock.Close);
    }

    [Fact]
    public async void GetBuyingOption_ShouldReturnNumberOfSharesForBudget()
    {
        // Arrange
        var data = LoadTestData();
        var mockService = new Mock<IStockDataService>();
        mockService.Setup(s => s.GetAll()).Returns(data);
        
        // Mock the cache to return null, simulating no cached data
        var mockCache = new Mock<IStockCacheService>();
        mockCache.Setup(s => s.GetStockAsync(It.IsAny<string>())).ReturnsAsync((StockData?)null);

        var controller = new StockController(mockService.Object, mockCache.Object);

        // Act
        var result = await controller.GetBuyingOption("AAPL", 1000) as OkObjectResult;

        // Assert
        Assert.NotNull(result);

        var json = JsonSerializer.Serialize(result?.Value);
        var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

        Assert.Equal("AAPL", parsed["Ticker"].GetString());
        Assert.Equal(1000m, parsed["Budget"].GetDecimal());
        Assert.Equal(4, parsed["Shares"].GetInt32()); // 1000 / 202.30 ≈ 4
    }

    // Test to ensure that GetAllTickers returns cached data if available
    [Fact]
    public async void GetAllTickers_ShouldReturnListOfTickers_from_Cache()
    {
        // Arrange
        var data = LoadTestData();

        var mockService = new Mock<IStockDataService>();

        var cachedTickers = new List<string> { "AAPL", "MSFT", "GOOGL" };
        var mockCache = new Mock<IStockCacheService>();
        mockCache.Setup(c => c.GetAllTickersAsync()).ReturnsAsync(cachedTickers);

        var controller = new StockController(mockService.Object, mockCache.Object);

        // Act
        var result = await controller.GetAllTickers() as OkObjectResult;

        // Assert
        var tickers = Assert.IsType<List<string>>(result?.Value);
        Assert.Contains("AAPL", tickers);
        Assert.Contains("MSFT", tickers);
        Assert.Contains("GOOGL", tickers);
        Assert.Equal(3, tickers.Count);
    }

    // Test to ensure that GetTickerDetails returns NotFound when ticker does not exist
    [Fact]
    public async Task GetTickerDetails_ShouldReturnNotFound_WhenTickerNotExists()
    {
        // Arrange        
        var data = LoadTestData();

        var mockService = new Mock<IStockDataService>();
        mockService.Setup(s => s.GetAll()).Returns(data);

        var mockCache = new Mock<IStockCacheService>();
        mockCache.Setup(s => s.GetStockAsync(It.IsAny<string>())).ReturnsAsync((StockData?)null);

        var controller = new StockController(mockService.Object, mockCache.Object);

        // Act
        var result = await controller.GetTickerDetails("NoTickerName");

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Ticker 'NoTickerName' not found.", notFoundResult.Value);
    }

    // Test to ensure that GetBuyingOption returns NotFound when ticker does not exist
    [Fact]
    public async Task GetBuyingOption_ShouldReturnNotFound_WhenTickerNotExists()
    {
        // Arrange
        var data = LoadTestData();

        var mockService = new Mock<IStockDataService>();
        mockService.Setup(s => s.GetAll()).Returns(data);

        var mockCache = new Mock<IStockCacheService>();
        mockCache.Setup(s => s.GetStockAsync(It.IsAny<string>())).ReturnsAsync((StockData?)null);

        var controller = new StockController(mockService.Object, mockCache.Object);

        // Act
        var result = await controller.GetBuyingOption("NoTickerName", 1000);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Ticker 'NoTickerName' not found.", notFoundResult.Value);
    }

    // Test to ensure that GetBuyingOption returns BadRequest when budget is zero or negative
    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task GetBuyingOption_ShouldReturnBadRequest_WhenBudgetIsZeroOrNegative(decimal invalidBudget)
    {
        // Arrange
        var mockService = new Mock<IStockDataService>();
        var mockCache = new Mock<IStockCacheService>();

        var controller = new StockController(mockService.Object, mockCache.Object);

        // Act
        var result = await controller.GetBuyingOption("AAPL", invalidBudget);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Budget must be greater than zero.", badRequestResult.Value);
    }
}