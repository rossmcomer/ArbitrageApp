using System.Text.Json;
using ArbitrageApp.Models;

namespace ArbitrageApp.Services
{
    public class KrakenService
    {
        private readonly HttpClient _client;

        public KrakenService()
        {
            _client = new HttpClient();
        }

        public async Task<List<CoinPriceModel>> GetActiveSymbols()
        {
            // Step 1: Get all tickers from Kraken
            HttpResponseMessage response = await _client.GetAsync("https://api.kraken.com/0/public/Ticker");
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error fetching tickers: {response.StatusCode}");
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseBody);
            var tickersElement = jsonDoc.RootElement.GetProperty("result");

            var tickers = new List<CoinPriceModel>();

            // Step 2: Extract tickers and prices
            foreach (var ticker in tickersElement.EnumerateObject())
            {
                string symbol = ticker.Name; // Kraken's trading pair (e.g., "XXBTZUSD")
                JsonElement tickerData = ticker.Value;

                // Kraken provides multiple prices; 'c' is the closing price
                string price = tickerData.GetProperty("c")[0].GetString() ?? string.Empty;

                tickers.Add(new CoinPriceModel
                {
                    Symbol = NormalizeKrakenSymbol(symbol),
                    Price = price
                });
            }

            // Step 3: Filter for USD and USDT pairs
            var usdTickers = tickers
                .Where(t => (t.Symbol?.EndsWith("USD") ?? false) || (t.Symbol?.EndsWith("USDT") ?? false))
                .ToList();

            return usdTickers;
        }

        private string NormalizeKrakenSymbol(string symbol)
        {
            // Normalize Kraken's symbol naming convention
            return symbol
                .Replace("XBT", "BTC")  // Kraken uses XBT instead of BTC
                .Replace("ZUSD", "USD") // Normalize USD pairs
                .Replace("ZUSDT", "USDT");
        }
    }
}
