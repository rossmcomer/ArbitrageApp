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
            // Step 1: Get active trading pairs with USD or USDC from Coinbase
            HttpResponseMessage productsResponse = await _client.GetAsync("https://api.exchange.coinbase.com/products");
            if (!productsResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error fetching Coinbase products: {productsResponse.StatusCode}");
            }

            string productsBody = await productsResponse.Content.ReadAsStringAsync();
            var productsJson = JsonDocument.Parse(productsBody);

            // Filter only active markets with USD or USDC as quote currency
            var activeSymbols = productsJson.RootElement
                .EnumerateArray()
                .Where(p =>
                    p.GetProperty("status").GetString() == "online" &&
                    (p.GetProperty("quote_currency").GetString() == "USD" ||
                     p.GetProperty("quote_currency").GetString() == "USDC")
                )
                .Select(p => p.GetProperty("id").GetString())
                .ToHashSet();

            if (activeSymbols.Count == 0)
            {
                return [];
            }

            Console.WriteLine(activeSymbols);

            // Step 2: Get current prices for each market using the ticker endpoint
            var tickers = new List<CoinPriceModel>();

            foreach (var symbol in activeSymbols)
            {
                HttpResponseMessage response = await _client.GetAsync($"https://api.exchange.coinbase.com/products/{symbol}/ticker");
                
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var priceJson = JsonDocument.Parse(responseBody);

                    var price = priceJson.RootElement.GetProperty("price").GetString();
                    tickers.Add(new CoinPriceModel
                    {
                        Symbol = symbol,
                        Price = price ?? "0"
                    });
                }
            }

            return tickers;
        }
    }
}