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
                HttpResponseMessage response = await _client.GetAsync("https://api.binance.us/api/v3/ticker/price?symbol=BTCUSDT");
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                JsonDocument jsonDoc = JsonDocument.Parse(responseBody);

                var coinPrice = new coinPriceModel
                {
                    Symbol = jsonDoc.RootElement.GetProperty("symbol").GetString(),
                    Price = jsonDoc.RootElement.GetProperty("price").GetString()
                };

                return Ok(coinPrice);
            }
            catch (Exception ex)
            {
                return Problem($"Error fetching data: {ex.Message}");
            }
        }
    }
}
