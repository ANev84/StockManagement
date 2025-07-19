using Microsoft.AspNetCore.Mvc;

namespace StockApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    [HttpGet("tickers")]
    public IActionResult GetAllTickers()
    {
        throw new NotImplementedException();
    }

    [HttpGet("{ticker}")]
    public IActionResult GetTickerDetails(string ticker)
    {
        throw new NotImplementedException();
    }

    [HttpGet("{ticker}/buy")]
    public IActionResult GetBuyingOption(string ticker, [FromQuery] decimal budget)
    {
        throw new NotImplementedException();
    }
}