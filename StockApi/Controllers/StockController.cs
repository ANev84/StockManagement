namespace StockApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using StockApi.StockDataService;
using System;
using System.Linq;


[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    private readonly IStockDataService _stockService;

    public StockController(IStockDataService stockService)
    {
        _stockService = stockService;
    }

    [HttpGet("tickers")]
    public IActionResult GetAllTickers()
    {
        var data = _stockService.GetAll();
        var tickers = data.Select(s => s.Ticker).Distinct().ToList();
        return Ok(tickers);
    }

    [HttpGet("{ticker}")]
    public IActionResult GetTickerDetails(string ticker)
    {
        var data = _stockService.GetAll();
        var result = data
            .Where(s => s.Ticker.Equals(ticker, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(s => s.Date)
            .ToList();

        if (!result.Any())
            return NotFound();

        return Ok(result);
    }

    [HttpGet("{ticker}/buy")]
    public IActionResult GetBuyingOption(string ticker, [FromQuery] decimal budget)
    {
        if (budget <= 0)
            return BadRequest("Budget must be greater than zero.");

        var data = _stockService.GetAll();
        var latest = data
            .Where(s => s.Ticker.Equals(ticker, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(s => s.Date)
            .FirstOrDefault();

        if (latest == null)
            return NotFound();

        var shares = Math.Floor(budget / latest.Close);

        return Ok(new
        {
            Ticker = latest.Ticker,
            Date = latest.Date.ToString("yyyy-MM-dd"),
            ClosePrice = latest.Close,
            Budget = budget,
            Shares = shares
        });
    }
}