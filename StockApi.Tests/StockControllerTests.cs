using Microsoft.AspNetCore.Mvc;
using StockApi.Controllers;

namespace StockApi.Tests
{
    public class StockControllerTests
    {
        [Fact]
        public void GetAllTickers_ShouldReturnListOfTickers()
        {
            // Arrange
            var controller = new StockController();

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
            var controller = new StockController();

            // Act
            var result = controller.GetTickerDetails("AAPL") as OkObjectResult;

            // Assert
            dynamic stock = result?.Value;
            Assert.NotNull(stock);
            Assert.Equal("AAPL", (string)stock.ticker);
            Assert.Equal(198.15m, (decimal)stock.open);
            Assert.Equal(202.30m, (decimal)stock.close);
        }

        [Fact]
        public void GetBuyingOption_ShouldReturnNumberOfSharesForBudget()
        {
            // Arrange
            var controller = new StockController();

            // Act
            var result = controller.GetBuyingOption("AAPL", 1000) as OkObjectResult;

            // Assert
            dynamic response = result?.Value;
            Assert.NotNull(response);
            Assert.Equal("AAPL", (string)response.ticker);
            Assert.Equal(1000, (decimal)response.budget);
            Assert.Equal(4, (int)response.shares); // 1000 / 202.30 ≈ 4
        }
    }
}