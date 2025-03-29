using ArbitrageApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArbitrageApp.Controllers
{
    [ApiController]
    [Route("api/tickers")]
    public class TickersController : ControllerBase
    {
        private readonly BinanceService _binanceService;
        private readonly CoinbaseService _coinbaseService;
        private readonly CryptoComService _cryptoComService;

        public TickersController(BinanceService binanceService, CoinbaseService coinbaseService, CryptoComService cryptoComService)
        {
            _binanceService = binanceService;
            _coinbaseService = coinbaseService;
            _cryptoComService = cryptoComService;
        }

        // Get tickers from Binance
        [HttpGet("binance")]
        public async Task<IActionResult> GetAllBinanceTickers()
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

        // Get tickers from Coinbase
        [HttpGet("coinbase")]
        public async Task<IActionResult> GetAllCoinbaseTickers()
        {
            try
            {
                var tickers = await _coinbaseService.GetAllTickers();
                return Ok(tickers);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Get tickers from Coinbase
        [HttpGet("cryptocom")]
        public async Task<IActionResult> GetAllCryptoComTickers()
        {
            try
            {
                var tickers = await _cryptoComService.GetAllTickers();
                return Ok(tickers);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}