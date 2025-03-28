using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ArbitrageApp.Models;

namespace ArbitrageApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArbitrageController(HttpClient client) : ControllerBase
    {
        private readonly HttpClient _client = client;

        [HttpGet("coin-price")]
        public async Task<IActionResult> GetCoinPrice()
        {
            try
            {
                var binancePrice = await GetBinancePrice();

                var coinbasePrice = await GetCoinbasePrice();

                var cryptoComPrice = await GetCryptoComPrice();

                var response = new
                {
                    BinancePrice = binancePrice,
                    CoinbasePrice = coinbasePrice,
                    CryptoComPrice = cryptoComPrice
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Problem($"Error fetching data: {ex.Message}");
            }
        }

        private async Task<CoinPriceModel> GetBinancePrice()
        {
            HttpResponseMessage response = await _client.GetAsync("https://api.binance.us/api/v3/ticker/price?symbol=BTCUSDT");
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            JsonDocument jsonDoc = JsonDocument.Parse(responseBody);

            return new CoinPriceModel
            {
                Symbol = jsonDoc.RootElement.GetProperty("symbol").GetString() ?? string.Empty,
                Price = jsonDoc.RootElement.GetProperty("price").GetString() ?? string.Empty
            };
        }

        private async Task<CoinPriceModel> GetCoinbasePrice()
        {
            HttpResponseMessage response = await _client.GetAsync("https://api.coinbase.com/v2/prices/spot?currency=USD");
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            JsonDocument jsonDoc = JsonDocument.Parse(responseBody);

            return new CoinPriceModel
            {
                Symbol = "BTCUSD",
                Price = jsonDoc.RootElement.GetProperty("data").GetProperty("amount").GetString() ?? string.Empty
            };
        }

        private async Task<CoinPriceModel> GetCryptoComPrice()
        {
            HttpResponseMessage response = await _client.GetAsync("https://api.crypto.com/exchange/v1/public/get-tickers?instrument_name=BTCUSD-PERP");
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            JsonDocument jsonDoc = JsonDocument.Parse(responseBody);

            var price = jsonDoc.RootElement
                       .GetProperty("result")
                       .GetProperty("data")
                       .EnumerateArray()
                       .FirstOrDefault()
                       .GetProperty("a")
                       .GetString();

            return new CoinPriceModel
            {
                Symbol = "BTCUSD-PERP",
                Price = price ?? string.Empty
            };
        }
    }
}
