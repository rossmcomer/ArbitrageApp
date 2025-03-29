using System.Text.Json;
using ArbitrageApp.Models;

namespace ArbitrageApp.Services
{
    public class CryptoComService
    {
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CryptoComService()
        {
            _client = new HttpClient();
        }

        public async Task<List<CoinPriceModel>> GetAllTickers()
        {
            // Step 1: Get all tickers from Crypto.com
            HttpResponseMessage response = await _client.GetAsync("https://api.crypto.com/v2/public/get-tickers");
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error fetching tickers: {response.StatusCode}");
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseBody);

            var tickers = jsonDoc.RootElement.GetProperty("result").GetProperty("data")
                .EnumerateArray()
                .Select(t => new CoinPriceModel
                {
                    Symbol = t.GetProperty("i").GetString() ?? string.Empty,
                    Price = t.GetProperty("a").GetString() ?? string.Empty
                })
                .ToList();

            // Step 2: Filter for USD and USDT pairs
            var usdTickers = tickers
                .Where(t => (t.Symbol?.EndsWith("USD") ?? false) || (t.Symbol?.EndsWith("USDT") ?? false ))
                .ToList();

            return usdTickers;
        }
    }
}