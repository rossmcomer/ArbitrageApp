using System.Text.Json;
using ArbitrageApp.Models;

namespace ArbitrageApp.Services
{
    public class CoinbaseService
    {
        private readonly HttpClient _client;

        public CoinbaseService()
        {
            _client = new HttpClient();
        }

        public async Task<HashSet<string>> GetActiveSymbols()
        {
            HttpResponseMessage productsResponse = await _client.GetAsync("https://api.exchange.coinbase.com/products");
            if (!productsResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error fetching Coinbase products: {productsResponse.StatusCode}");
            }

            string productsBody = await productsResponse.Content.ReadAsStringAsync();
            var productsJson = JsonDocument.Parse(productsBody);

            var activeSymbols = productsJson.RootElement
                .EnumerateArray()
                .Where(p =>
                    p.GetProperty("status").GetString() == "online" &&
                    (p.GetProperty("quote_currency").GetString() == "USD" ||
                     p.GetProperty("quote_currency").GetString() == "USDC")
                )
                .Select(p => p.GetProperty("id").GetString())
                .Where(id => id is not null)
                .Select(id => id!)
                .ToHashSet();
                
            return activeSymbols;
        }

        public async Task<List<CoinPriceModel>> GetPricesForSymbols(IEnumerable<string> symbols)
        {
            var tickers = new List<CoinPriceModel>();

            foreach (var symbol in symbols)
            {
                var formattedSymbol = symbol.Replace("-", "");

                HttpResponseMessage response = await _client.GetAsync($"https://api.exchange.coinbase.com/products/{symbol}/ticker");

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var priceJson = JsonDocument.Parse(responseBody);

                    var price = priceJson.RootElement.GetProperty("price").GetString();
                    tickers.Add(new CoinPriceModel
                    {
                        Symbol = formattedSymbol,
                        Price = price ?? "0"
                    });
                }
            }

            return tickers;
        }
    }
}