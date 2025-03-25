using System.Net.WebSockets;
using System.Text;

public class WebSocketService
{
    private readonly ClientWebSocket _clientWebSocket = new ClientWebSocket();
    private readonly string _uri;

    public WebSocketService(string uri)
    {
        _uri = uri;
    }

    public async Task ConnectAsync()
    {
        try
        {
            Console.WriteLine($"Connecting to {_uri}...");
            await _clientWebSocket.ConnectAsync(new Uri(_uri), CancellationToken.None);
            Console.WriteLine("Connected.");

            await ReceiveMessagesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
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
        }
    }
}
