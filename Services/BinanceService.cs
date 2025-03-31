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

        // Function 1: Fetch Active Binance Symbols
        public async Task<HashSet<string>> GetActiveSymbols()
        {
            // Get active trading pairs
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
                .Where(id => id is not null)
                .Select(id => id!)
                .ToHashSet();

            return activeSymbols;
        }

        // Function 2: Get Current Prices for Active Symbols
        public async Task<List<CoinPriceModel>> GetPricesForSymbols(HashSet<string> activeSymbols)
        {
            // Get current prices for the active symbols
            HttpResponseMessage response = await _client.GetAsync("https://api.binance.us/api/v3/ticker/price");
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error fetching tickers: {response.StatusCode}");
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            var tickers = JsonSerializer.Deserialize<List<CoinPriceModel>>(responseBody, _jsonOptions) ?? new List<CoinPriceModel>();

            // Filter tickers to only include the ones that are in the activeSymbols set
            var activeTickers = tickers
                .Where(t => t.Symbol != null && activeSymbols.Contains(t.Symbol))
                .ToList();

            // Filter for USD and USDT pairs (optional)
            var usdTickers = activeTickers
                .Where(t => (t.Symbol?.EndsWith("USD") ?? false) || (t.Symbol?.EndsWith("USDT") ?? false))
                .ToList();

            return usdTickers;
        }
    }
}