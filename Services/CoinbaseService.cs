using System.Text.Json;
using ArbitrageApp.Models;

namespace ArbitrageApp.Services
{
    public class CoinbaseService
    {
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CoinbaseService()
        {
            _client = new HttpClient();
        }

        public async Task<List<CoinPriceModel>> GetAllTickers()
        {
            // Step 1: Get active trading pairs from Coinbase
            HttpResponseMessage productsResponse = await _client.GetAsync("https://api.exchange.coinbase.com/products");
            if (!productsResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error fetching Coinbase products: {productsResponse.StatusCode}");
            }

            string productsBody = await productsResponse.Content.ReadAsStringAsync();
            var productsJson = JsonDocument.Parse(productsBody);

            // Filter only active markets
            var activeSymbols = productsJson.RootElement
                .EnumerateArray()
                .Where(p => p.GetProperty("status").GetString() == "online") // Only active markets
                .Select(p => p.GetProperty("id").GetString()) // Get the market symbol
                .ToHashSet();

            // Step 2: Get current prices from the ticker endpoint
            HttpResponseMessage response = await _client.GetAsync("https://api.exchange.coinbase.com/products/ticker");
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
                .Where(t => (t.Symbol?.EndsWith("-USD") ?? false) || (t.Symbol?.EndsWith("-USDC") ?? false))
                .ToList();

            return usdTickers;
        }
    }
}