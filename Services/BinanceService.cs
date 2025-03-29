using System.Text.Json;
using ArbitrageApp.Models;

namespace ArbitrageApp.Services
{
    public class BinanceService
    {
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public BinanceService()
        {
            _client = new HttpClient();
        }

        public async Task<List<CoinPriceModel>> GetAllTickers()
        {
            // Step 1: Get active trading pairs
            HttpResponseMessage exchangeInfoResponse = await _client.GetAsync("https://api.binance.us/api/v3/exchangeInfo");
            if (!exchangeInfoResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error fetching exchange info: {exchangeInfoResponse.StatusCode}");
            }

            string exchangeInfoBody = await exchangeInfoResponse.Content.ReadAsStringAsync();
            var exchangeInfoJson = JsonDocument.Parse(exchangeInfoBody);

            var activeSymbols = exchangeInfoJson.RootElement
                .GetProperty("symbols")
                .EnumerateArray()
                .Where(s => s.GetProperty("status").GetString() == "TRADING")
                .Select(s => s.GetProperty("symbol").GetString())
                .ToHashSet();

            // Step 2: Get current prices from the ticker endpoint
            HttpResponseMessage response = await _client.GetAsync("https://api.binance.us/api/v3/ticker/price");
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error fetching tickers: {response.StatusCode}");
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            var tickers = JsonSerializer.Deserialize<List<CoinPriceModel>>(responseBody, _jsonOptions) ?? new List<CoinPriceModel>();

            // Step 3: Filter tickers to only include active symbols
            var activeTickers = tickers
                .Where(t => t.Symbol != null && activeSymbols.Contains(t.Symbol))
                .ToList();

            // Step 4: Filter for USD and USDT pairs
            var usdTickers = activeTickers
                .Where(t => (t.Symbol?.EndsWith("USD") ?? false) || (t.Symbol?.EndsWith("USDT") ?? false))
                .ToList();

            return usdTickers;
        }
    }
}