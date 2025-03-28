using System.Text.Json;
using ArbitrageApp.Models;

public class BinanceService
{
    private readonly HttpClient _client;

    public BinanceService()
    {
        _client = new HttpClient();
    }

    public async Task<List<CoinPriceModel>> GetAllTickers()
    {
        // Fetch all tickers from Binance.US
        HttpResponseMessage response = await _client.GetAsync("https://api.binance.us/api/v3/ticker/price");
        
        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var tickers = JsonSerializer.Deserialize<List<CoinPriceModel>>(responseBody, options)?? new List<CoinPriceModel>();

            
            return tickers;   
                                
        }
        else
        {
            // If the request fails, you can throw an exception or return an empty list.
            throw new HttpRequestException($"Error fetching tickers: {response.StatusCode}");
        }
    }
}