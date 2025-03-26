var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Add your WebSocket connection code here
var binanceService = new BinanceWebSocketService();
var coinbaseService = new CoinbaseWebSocketService();
var cryptoComService = new CryptoComWebSocketService();

// Start the WebSocket connections when the application starts
app.Lifetime.ApplicationStarted.Register(async () =>
{
    await binanceService.ConnectAsync();
    await binanceService.SubscribeToTicker("btcusdt");

    // await coinbaseService.ConnectAsync();
    // await coinbaseService.SubscribeToTicker("BTC-USD");

    // await cryptoComService.ConnectAsync();
    // await cryptoComService.SubscribeToTicker("BTC_USDT");
});

app.MapGet("/", () => "Welcome to ArbitrageApp!");

// Example Endpoint: Check Binance Connection
app.MapGet("/status/binance", () => new
{
    Status = binanceService.IsConnected ? "Connected" : "Disconnected"
});

app.MapPost("/subscribe/binance/{symbol}", async (string symbol) =>
{
    if (!binanceService.IsConnected)
    {
        return Results.BadRequest("Not connected to Binance.");
    }

    await binanceService.SubscribeToTicker(symbol);
    return Results.Ok($"Subscribed to {symbol} on Binance.");
});

app.Run();
