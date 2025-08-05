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
    private StockController _controller;

    public StockControllerTests()
    {
        _testData = LoadTestData();
        _controller = SetupController();
    }

    // Load test data from a JSON file
    private List<StockData> LoadTestData()
    {
        var json = File.ReadAllText("test_stocks.json");
        return JsonSerializer.Deserialize<List<StockData>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Test data file 'test_stocks.json' could not be loaded.");
    }

    // Default setup for the controller
    private StockController SetupController(
        List<StockData>? data = null,
        List<StockData>? cachedTickers = null,
        StockData? cachedStock = null)
    {
        var mockService = new Mock<IStockDataService>();
        mockService.Setup(s => s.GetAll()).Returns(data ?? _testData);

        var mockCache = new Mock<IStockCacheService>();
        mockCache.Setup(c => c.GetAllTickersAsync()).ReturnsAsync(cachedTickers);
        mockCache.Setup(c => c.GetStockAsync(It.IsAny<string>())).ReturnsAsync(cachedStock);

        var controller = new StockController(mockService.Object, mockCache.Object);

        return controller;
    }

    [Fact]
    public async Task GetAllTickers_ShouldReturnListOfTickers_WhenCacheIsEmpty()
    {
        SetupController(cachedTickers: null); // redefine

        var result = await _controller.GetAllTickers() as OkObjectResult;

        var tickers = Assert.IsType<List<StockData>>(result?.Value);
        Assert.Contains(tickers, t => t.Ticker == Aapl);
        Assert.Contains(tickers, t => t.Ticker == "MSFT");
        Assert.Contains(tickers, t => t.Ticker == "GOOGL");
        Assert.Equal(3, tickers.Count);
    }

    [Fact]
    public async Task GetAllTickers_ShouldReturnListOfTickers_FromCache()
    {
        SetupController(cachedTickers: new List<StockData>
        {
            new StockData("AAPL", DateTime.Today, 0, 0, 0, 0, 0),
            new StockData("MSFT", DateTime.Today, 0, 0, 0, 0, 0),
            new StockData("GOOGL", DateTime.Today, 0, 0, 0, 0, 0)
        });

        var result = await _controller.GetAllTickers() as OkObjectResult;

        var tickers = Assert.IsType<List<StockData>>(result?.Value);
        Assert.Equal(3, tickers.Count);
    }

    [Theory]
    [InlineData(Aapl, true)]
    [InlineData("UnknownTicket", false)]
    public async Task GetTickerDetails_ShouldReturnCorrectDetails_or_ShouldReturnNotFound_WhenTickerDoesNotExist(string ticket, bool isExist)
    {
        // The standard _controller is used
        var result = await _controller.GetTickerDetails(ticket);

        if (isExist)
        {
            var okResult = Assert.IsType<OkObjectResult>(result);
            var stock = Assert.IsType<StockData>(okResult?.Value);

            Assert.Equal(Aapl, stock.Ticker);
            Assert.Equal(198.15m, stock.Open);
            Assert.Equal(202.30m, stock.Close);
        }
        else
        {
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Ticker '{ticket}' not found.", notFound.Value);
        }
    }

    [Theory]
    [InlineData(Aapl, true)]
    [InlineData("UnknownTicket", false)]
    public async Task GetBuyingOption_ReturnCorrectShareCount_or_ShouldReturnNotFound_WhenTickerDoesNotExist(string ticket, bool isExist)
    {
        var result = await _controller.GetBuyingOption(ticket, 1000);

        if (isExist)
        {
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var json = JsonSerializer.Serialize(okResult.Value);
            var parsed = JsonSerializer.Deserialize<JsonNode>(json);

            Assert.Equal("AAPL", parsed?["Ticker"]?.ToString());
            Assert.Equal("1000", parsed?["Budget"]?.ToString());
            Assert.Equal("4", parsed?["Shares"]?.ToString());
        }
        else
        {
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Ticker '{ticket}' not found.", notFound.Value);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task GetBuyingOption_ShouldReturnBadRequest_WhenBudgetInvalid(decimal budget)
    {
        var result = await _controller.GetBuyingOption(Aapl, budget);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Budget must be greater than zero.", badRequest.Value);
    }
}
