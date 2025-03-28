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
            HttpResponseMessage response = await _client.GetAsync("https://api.binance.us/api/v3/ticker/price");
            
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                
                var tickers = JsonSerializer.Deserialize<List<CoinPriceModel>>(responseBody, _jsonOptions) ?? [];

                
                return tickers;   
                                    
            }
            else
            {
                throw new HttpRequestException($"Error fetching tickers: {response.StatusCode}");
            }
        }
    }
}