using Microsoft.AspNetCore.Mvc;
using Moq;
using StockApi.Controllers;
using StockApi.Models;
using StockApi.StockDataService;
using StockApi.StockDataService.Cache;
using System.Text.Json;
using System.Text.Json.Nodes;


public class StockControllerTests
{
    private const string Aapl = "AAPL";
    private readonly List<StockData> _testData;

    public StockControllerTests()
    {
        _testData = LoadTestData();
    }

    private List<StockData> LoadTestData()
    {
        var json = File.ReadAllText("test_stocks.json");
        return JsonSerializer.Deserialize<List<StockData>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Test data file 'test_stocks.json' could not be loaded.");
    }

    private StockController SetupController(
        List<StockData>? data = null,
        List<string>? cachedTickers = null,
        StockData? cachedStock = null)
    {
        var mockService = new Mock<IStockDataService>();
        mockService.Setup(s => s.GetAll()).Returns(data ?? _testData);

        var mockCache = new Mock<IStockCacheService>();
        mockCache.Setup(c => c.GetAllTickersAsync()).ReturnsAsync(cachedTickers);
        mockCache.Setup(c => c.GetStockAsync(It.IsAny<string>())).ReturnsAsync(cachedStock);

        return new StockController(mockService.Object, mockCache.Object);
    }

    [Fact]
    public async Task GetAllTickers_ShouldReturnListOfTickers_WhenCacheIsEmpty()
    {
        var controller = SetupController(cachedTickers: null);

        var result = await controller.GetAllTickers() as OkObjectResult;

        var tickers = Assert.IsType<List<string>>(result?.Value);
        Assert.Contains(Aapl, tickers);
        Assert.Contains("MSFT", tickers);
        Assert.Contains("GOOGL", tickers);
        Assert.Equal(3, tickers.Count);
    }

    [Fact]
    public async Task GetAllTickers_ShouldReturnListOfTickers_FromCache()
    {
        var controller = SetupController(cachedTickers: new List<string> { Aapl, "MSFT", "GOOGL" });

        var result = await controller.GetAllTickers() as OkObjectResult;

        var tickers = Assert.IsType<List<string>>(result?.Value);
        Assert.Equal(3, tickers.Count);
    }

    [Fact]
    public async Task GetTickerDetails_ShouldReturnCorrectDetails()
    {
        var controller = SetupController();

        var result = await controller.GetTickerDetails(Aapl) as OkObjectResult;
        var stock = Assert.IsType<StockData>(result?.Value);

        Assert.Equal(Aapl, stock.Ticker);
        Assert.Equal(198.15m, stock.Open);
        Assert.Equal(202.30m, stock.Close);
    }

    [Fact]
    public async Task GetTickerDetails_ShouldReturnNotFound_WhenTickerDoesNotExist()
    {
        var controller = SetupController();

        var result = await controller.GetTickerDetails("UnknownTicker");

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Ticker 'UnknownTicker' not found.", notFound.Value);
    }

    [Fact]
    public async Task GetBuyingOption_ShouldReturnCorrectShareCount()
    {
        var controller = SetupController();

        var result = await controller.GetBuyingOption(Aapl, 1000) as OkObjectResult;

        Assert.NotNull(result);

        var json = JsonSerializer.Serialize(result.Value);
        var parsed = JsonSerializer.Deserialize<JsonNode>(json);

        Assert.Equal(Aapl, parsed?["Ticker"]?.ToString());
        Assert.Equal("1000", parsed?["Budget"]?.ToString());
        Assert.Equal("4", parsed?["Shares"]?.ToString());
    }

    [Fact]
    public async Task GetBuyingOption_ShouldReturnNotFound_WhenTickerDoesNotExist()
    {
        var controller = SetupController();

        var result = await controller.GetBuyingOption("XYZ", 1000);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Ticker 'XYZ' not found.", notFound.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task GetBuyingOption_ShouldReturnBadRequest_WhenBudgetInvalid(decimal budget)
    {
        var controller = SetupController();

        var result = await controller.GetBuyingOption(Aapl, budget);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Budget must be greater than zero.", badRequest.Value);
    }
}