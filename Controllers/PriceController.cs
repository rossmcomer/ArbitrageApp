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
        public async Task<IActionResult> GetBtcPrice()
        {
            try
            {
                var binancePrice = await GetBinancePrice();

                var coinbasePrice = await GetCoinbasePrice();

                var response = new
                {
                    BinancePrice = binancePrice,
                    CoinbasePrice = coinbasePrice
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
                Symbol = "BTCUSD", // Coinbase gives price in USD for BTC, so we can just set this manually
                Price = jsonDoc.RootElement.GetProperty("data").GetProperty("amount").GetString() ?? string.Empty
            };
        }
    }
}
