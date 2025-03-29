using ArbitrageApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<BinanceService>();
builder.Services.AddSingleton<CoinbaseService>();
builder.Services.AddSingleton<CryptoComService>();
builder.Services.AddHttpClient();
builder.Services.AddControllers();

var app = builder.Build();

app.MapGet("/", () => "Welcome to ArbitrageApp!");

app.MapControllers();

app.Run();
