var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Start the WebSocket connections when the application starts
app.Lifetime.ApplicationStarted.Register(async () =>
{
    
});

app.MapGet("/", () => "Welcome to ArbitrageApp!");

app.Run();
