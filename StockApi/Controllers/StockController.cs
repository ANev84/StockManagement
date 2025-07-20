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
    public virtual async Task<IActionResult> GetAllTickers()
    {
        var cachedTickers = await _stockCache.GetAllTickersAsync();
        if (cachedTickers != null)
            return Ok(cachedTickers);

        var data = _stockService.GetAll();
        var tickers = data.Select(s => s.Ticker).Distinct().ToList();

        await _stockCache.SetAllTickersAsync(tickers);
        return Ok(tickers);
    }

    [HttpGet("{ticker}")]
    public async Task<IActionResult> GetTickerDetails(string ticker)
    {
        var cached = await _stockCache.GetStockAsync(ticker);
        if (cached != null)
            return Ok(cached);

        var data = _stockService.GetAll();
        if (data == null || !data.Any())
            return NotFound("No stock data available.");

        StockData? result = data.FirstOrDefault(s => s.Ticker.Equals(ticker, StringComparison.OrdinalIgnoreCase));
            
        await _stockCache.SetStockAsync(ticker, result);
        return Ok(result);
    }

    [HttpGet("{ticker}/buy")]
    public async Task<IActionResult> GetBuyingOption(string ticker, [FromQuery] decimal budget)
    {
        if (budget <= 0)
            return BadRequest("Budget must be greater than zero.");

        var cached = await _stockCache.GetStockAsync(ticker);
        StockData? latest = null;
        if (cached != null) {
            latest = cached;
        }            
        else
        {
            var dataFromFile = _stockService.GetAll();

            if (dataFromFile == null || !dataFromFile.Any())
                return NotFound("No stock data available.");

            latest = dataFromFile.FirstOrDefault(s => s.Ticker.Equals(ticker, StringComparison.OrdinalIgnoreCase));
        }
        
        if(latest == null)
            return NotFound($"Ticker '{ticker}' not found.");

        var shares = Math.Floor(budget / latest.Close);

        return Ok(new
        {
            latest.Ticker,           
            Budget = budget,
            Shares = shares
        });
    }
}