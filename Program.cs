using ArbitrageApp.Services;
using ArbitrageApp.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<BinanceService>();
builder.Services.AddSingleton<CoinbaseService>();
builder.Services.AddSingleton<CryptoComService>();
builder.Services.AddSingleton<KrakenService>();
builder.Services.AddHttpClient();
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapGet("/", () => "Welcome to ArbitrageApp!");

app.MapControllers();

app.Run();
