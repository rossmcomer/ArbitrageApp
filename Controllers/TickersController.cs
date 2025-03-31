using ArbitrageApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArbitrageApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        [HttpGet("arbitrage")]
        public async Task GetArbitrageOpportunities()
        {
            
                var binanceResponse = await _binanceService.GetActiveSymbols();

                var cryptoComResponse = await _cryptoComService.GetActiveSymbols();

                var coinbaseTickers = await _coinbaseService.GetActiveSymbols();

                Console.WriteLine(string.Join(", ", binanceResponse));

                
            
        }

        // Get tickers from Binance
        [HttpGet("binance")]
        public async Task<IActionResult> GetAllBinanceTickers()
        {
            try
            {
                var tickers = await _binanceService.GetActiveSymbols();
                var tickersWithPrices = await _binanceService.GetPricesForSymbols(tickers);
                return Ok(tickersWithPrices);
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
                var tickers = await _coinbaseService.GetActiveSymbols();

                var tickersWithPrices = await _coinbaseService.GetPricesForSymbols(tickers);

                return Ok(tickersWithPrices);
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
                var tickers = await _cryptoComService.GetActiveSymbols();

                return Ok(tickers);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}