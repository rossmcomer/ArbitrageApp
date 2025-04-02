namespace ArbitrageApp.Models
{
    public class CoinPriceModel
    {
        public string? Symbol { get; set; }
        public string? Price { get; set; }
    }

    public class ArbitrageOpportunity
{
    public string? BinancePrice { get; set; }
    public string? CoinbasePrice { get; set; }
    public string? CryptoComPrice { get; set; }
    public string? KrakenPrice { get; set; }
    public decimal PercentDiff { get; set; }
}
}