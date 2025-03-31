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
        public async Task<IActionResult> GetArbitrageOpportunities()
        {
            try
            {
                var binanceResponse = await GetAllBinanceTickers();

                // var coinbaseTickers = await _coinbaseService.GetAllTickers();

                var cryptoComResponse = await GetAllCryptoComTickers();

                if (binanceResponse is not OkObjectResult binanceResult || cryptoComResponse is not OkObjectResult cryptoComResult)
                {
                    return StatusCode(500, new { message = "Failed to retrieve tickers from one or both sources." });
                }

                if (binanceResult.Value is not IEnumerable<dynamic> binanceTickers || cryptoComResult.Value is not IEnumerable<dynamic> cryptoComTickers)
                {
                    return StatusCode(500, new { message = "Invalid ticker data format." });
                }

                static string Normalize(string symbol) => System.Text.RegularExpressions.Regex.Replace(symbol, "(USD|USDC|USDT)$", "");

                var binanceDict = binanceTickers
                    .GroupBy(t => Normalize(t.Symbol))
                    .ToDictionary(g => g.Key, g => g.OrderBy(t => Convert.ToDecimal(t.Price)).First());

                var cryptoComDict = cryptoComTickers
                    .GroupBy(t => Normalize(t.Symbol))
                    .ToDictionary(g => g.Key, g => g.OrderBy(t => Convert.ToDecimal(t.Price)).First());

                // Find matching symbols and prepare result
                var commonSymbols = binanceDict.Keys.Intersect(cryptoComDict.Keys);

                var result = commonSymbols.Select(symbol => new
                {
                    Symbol = symbol,
                    BinancePrice = binanceDict[symbol].Price,
                    CryptoComPrice = cryptoComDict[symbol].Price
                }).ToList();

                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
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

                var formattedTickers = tickers.Select(ticker => new
                {
                    Symbol = ticker.Symbol?.Replace("-", ""),
                    ticker.Price
                }).ToList();

                return Ok(formattedTickers);
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

                var formattedTickers = tickers.Select(ticker => new
                {
                    Symbol = ticker.Symbol?.Replace("_", ""),
                    ticker.Price
                }).ToList();

                return Ok(formattedTickers);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}