var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<BinanceService>();
builder.Services.AddHttpClient();
builder.Services.AddControllers();

var app = builder.Build();

app.MapGet("/", () => "Welcome to ArbitrageApp!");

app.MapControllers();

app.Run();
