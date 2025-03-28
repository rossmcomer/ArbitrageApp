using ArbitrageApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArbitrageApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TickersController(BinanceService binanceService) : ControllerBase
    {
        private readonly BinanceService _binanceService = binanceService;

        [HttpGet("binancetickers")]
        public async Task<IActionResult> GetAllTickers()
        {
            try
            {
                var tickers = await _binanceService.GetAllTickers();

                return Ok(tickers);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}