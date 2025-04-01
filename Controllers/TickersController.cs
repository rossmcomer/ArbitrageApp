using ArbitrageApp.Services;
using Microsoft.AspNetCore.Mvc;
using ArbitrageApp.Models;

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
            string Normalize(string symbol) => System.Text.RegularExpressions.Regex.Replace(symbol, "(USD|USDC|USDT)$", ""); 
            
            var binanceSymbols = await _binanceService.GetActiveSymbols(); //[BTCUSDT, ETCUSD]

            var coinbaseSymbolsPreFormat = await _coinbaseService.GetActiveSymbols(); //[BTC-USDT, ETC-USD]

            var cryptoComSymbolsPreFormat = await _cryptoComService.GetActiveSymbols(); //[{"symbol": "BTCUSDT", "price": "0.021590"},{"symbol": "ETHUSDT", "price": "0.040090"}]

            var coinbaseSymbolMap = coinbaseSymbolsPreFormat
                .GroupBy(symbol => Normalize(symbol.Replace("-", "")))  // Normalize and group by symbol
                .ToDictionary(group => group.Key, group => group.First());

            var cryptoComSymbols = cryptoComSymbolsPreFormat
                .Select(item => Normalize(item.Symbol ?? string.Empty))  // Normalize Crypto.com symbols
                .ToHashSet();

            var allSymbols = binanceSymbols
                .Select(symbol => Normalize(symbol))  // Normalize Binance symbols
                .Concat(coinbaseSymbolMap.Keys)       // Use the normalized Coinbase symbols
                .Concat(cryptoComSymbols)             // Add the normalized Crypto.com symbols
                .GroupBy(symbol => symbol)           // Group by normalized symbol
                .Where(group => group.Count() >= 2)  // Filter symbols that appear in 2 or more sources
                .Select(group => new
                {
                    NormalizedSymbol = group.Key,
                    OriginalCoinbaseSymbol = group.Key != null && coinbaseSymbolMap.TryGetValue(group.Key, out string? value) 
                        ? value : null
                })
                .ToArray();
            
            var coinbaseSymbolsToFetch = allSymbols
                .Where(x => x.OriginalCoinbaseSymbol != null)
                .Select(x => x.OriginalCoinbaseSymbol!)
                .ToList();
            
            var coinbasePrices = await _coinbaseService.GetPricesForSymbols(coinbaseSymbolsToFetch);

            var binanceSymbolsToFetch = binanceSymbols
                .Where(symbol => allSymbols.Any(s => s.NormalizedSymbol == Normalize(symbol)))
                .ToHashSet();

            var binancePrices = await _binanceService.GetPricesForSymbols(binanceSymbolsToFetch);

            var finalDictionary = allSymbols.ToDictionary(
                x => x.NormalizedSymbol,
                x => new ArbitrageOpportunity
                {
                    BinancePrice = binancePrices
                        .FirstOrDefault(b => Normalize(b.Symbol ?? string.Empty) == x.NormalizedSymbol)?.Price,
                    CoinbasePrice = coinbasePrices
                        .FirstOrDefault(c => Normalize(c.Symbol ?? string.Empty) == x.NormalizedSymbol)?.Price,
                    CryptoComPrice = cryptoComSymbolsPreFormat
                        .FirstOrDefault(cc => Normalize(cc.Symbol ?? string.Empty) == x.NormalizedSymbol)?.Price,
                    PercentDiff = 0m
                }
            );

            var arbitrageOpportunities = finalDictionary
                .Where(kv =>
                {
                    bool TryParsePrice(string? priceStr, out decimal price) =>
                        decimal.TryParse(priceStr, out price);

                    var prices = new List<decimal>();

                    if (TryParsePrice(kv.Value.BinancePrice, out var binancePrice)) prices.Add(binancePrice);
                    if (TryParsePrice(kv.Value.CoinbasePrice, out var coinbasePrice)) prices.Add(coinbasePrice);
                    if (TryParsePrice(kv.Value.CryptoComPrice, out var cryptoComPrice)) prices.Add(cryptoComPrice);

                    if (prices.Count < 2)
                        return false; // Need at least 2 prices to compare

                    var minPrice = prices.Min();
                    var maxPrice = prices.Max();

                    var percentDiff = (maxPrice - minPrice) / minPrice * 100; // Check if difference is 1% or more

                    kv.Value.PercentDiff = percentDiff;

                    return percentDiff >= 1;
                })
                .OrderByDescending(kv => kv.Value.PercentDiff) // Sort by percentDiff in descending order
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            return Ok(arbitrageOpportunities);            
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