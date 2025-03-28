using System.Net.WebSockets;
using System.Text;

public class WebSocketService
{
    protected readonly ClientWebSocket _clientWebSocket = new ClientWebSocket();
    private readonly string _uri;

    public bool IsConnected { get; private set; } = false;

    private readonly System.Timers.Timer _timer;

    public WebSocketService(string uri)
    {
        _uri = uri;
        _timer = new System.Timers.Timer(600000); // 10 minutes in milliseconds
        _timer.Elapsed += async (sender, e) => await OnTimerElapsedAsync();
    }

    public async Task ConnectAsync()
    {
        try
        {
            Console.WriteLine($"Connecting to {_uri}...");
            await _clientWebSocket.ConnectAsync(new Uri(_uri), CancellationToken.None);
            Console.WriteLine("Connected.");

            IsConnected = _clientWebSocket.State == WebSocketState.Open;

            if (IsConnected)
            {
                Console.WriteLine("Starting timer for periodic updates...");
                _timer.Start();
            }

            await ReceiveMessagesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    protected virtual async Task OnTimerElapsedAsync()
    {
        if (IsConnected)
        {
            Console.WriteLine("Timer elapsed. Sending periodic request...");
            await SendPeriodicMessageAsync();
        }
        else
        {
            Console.WriteLine("WebSocket is not connected. Skipping periodic request.");
        }
    }

    protected virtual Task SendPeriodicMessageAsync()
    {
        Console.WriteLine("Periodic message sending not implemented in base class.");
        return Task.CompletedTask;
    }

    private async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[4096];

        while (_clientWebSocket.State == WebSocketState.Open)
        {
            var result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            Console.WriteLine($"Received: {message}");
        }
    }

    public async Task DisconnectAsync()
    {
        if (_clientWebSocket.State == WebSocketState.Open)
        {
            await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            Console.WriteLine("Disconnected.");
            _timer.Stop();
        }
    }
}
