namespace StockApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using StockApi.Models;
using StockApi.StockDataService;
using StockApi.StockDataService.Cache;
using System;
using System.Linq;


[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    private readonly IStockDataService _stockService;
    private readonly IStockCacheService _stockCache;

    public StockController(IStockDataService stockService, IStockCacheService stockCache)
    {
        _stockService = stockService;
        _stockCache = stockCache;
    }

    [HttpGet("tickers")]
    public async Task<IActionResult> GetAllTickers()
    {
        var tickers = await _stockCache.GetAllTickersAsync();
        if (tickers == null)
        {
            tickers = _stockService
                .GetAll()
                .Select(s => s.Ticker)
                .Distinct()
                .ToList();

            await _stockCache.SetAllTickersAsync(tickers);
        }

        return Ok(tickers);
    }

    [HttpGet("{ticker}")]
    public async Task<IActionResult> GetTickerDetails(string ticker)
    {
        var stock = await GetOrLoadStockAsync(ticker);
        if (stock == null)
            return NotFound($"Ticker '{ticker}' not found.");

        return Ok(stock);
    }

    [HttpGet("{ticker}/buy")]
    public async Task<IActionResult> GetBuyingOption(string ticker, [FromQuery] decimal budget)
    {
        if (budget <= 0)
            return BadRequest("Budget must be greater than zero.");

        var stock = await GetOrLoadStockAsync(ticker);
        if (stock == null)
            return NotFound($"Ticker '{ticker}' not found.");

        var shares = Math.Floor(budget / stock.Close);

        return Ok(new
        {
            stock.Ticker,
            Budget = budget,
            Shares = shares
        });
    }

    /// <summary>
    /// Get stock from cache or download from source and save to cache
    /// </summary>
    private async Task<StockData?> GetOrLoadStockAsync(string ticker)
    {
        var cached = await _stockCache.GetStockAsync(ticker);
        if (cached != null)
            return cached;

        var data = _stockService.GetAll();
        if (data == null || !data.Any())
            return null;

        var stock = data.FirstOrDefault(s => s.Ticker.Equals(ticker, StringComparison.OrdinalIgnoreCase));
        if (stock != null)
            await _stockCache.SetStockAsync(ticker, stock);

        return stock;
    }
}