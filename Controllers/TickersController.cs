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
                var binanceTickers = (await _binanceService.GetAllTickers())
                    .Where(x => !string.IsNullOrEmpty(x.Symbol))
                    .ToDictionary(x => x.Symbol!, x => x.Price);

                var coinbaseTickers = (await _coinbaseService.GetAllTickers())
                    .Where(x => !string.IsNullOrEmpty(x.Symbol))
                    .ToDictionary(x => x.Symbol!, x => x.Price);

                var cryptoComTickers = (await _cryptoComService.GetAllTickers())
                    .Where(x => !string.IsNullOrEmpty(x.Symbol))
                    .ToDictionary(x => x.Symbol!, x => x.Price);

                var allSymbols = binanceTickers.Keys
                                .Union(coinbaseTickers.Keys)
                                .Union(cryptoComTickers.Keys);

                Console.WriteLine(allSymbols);

                // var opportunities = new List<object>();

                // foreach (var symbol in allSymbols)
                // {
                //     binanceTickers.TryGetValue(symbol, out var binancePrice);
                //     coinbaseTickers.TryGetValue(symbol, out var coinbasePrice);
                //     cryptoComTickers.TryGetValue(symbol, out var cryptoComPrice);

                //     var prices = new List<decimal?> { binancePrice, coinbasePrice, cryptoComPrice }
                //                     .Where(p => p.HasValue)
                //                     .Select(p => p.Value)
                //                     .ToList();

                //     if (prices.Count < 2) continue; // Skip if there are not enough prices to compare

                //     var minPrice = prices.Min();
                //     var maxPrice = prices.Max();
                //     var percentDiff = ((maxPrice - minPrice) / minPrice) * 100;

                //     if (percentDiff >= 1)
                //     {
                //         opportunities.Add(new
                //         {
                //             Symbol = symbol,
                //             BinancePrice = binancePrice,
                //             CoinbasePrice = coinbasePrice,
                //             CryptoComPrice = cryptoComPrice,
                //             PercentDifference = percentDiff
                //         });
                //     }
                // }

                return Ok(allSymbols);
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