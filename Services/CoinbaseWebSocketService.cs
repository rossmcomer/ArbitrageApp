using System.Net.WebSockets;
using System.Text;

public class CoinbaseWebSocketService : WebSocketService
{
    public CoinbaseWebSocketService() : base("wss://ws-feed.exchange.coinbase.com")
    {
    }

    public async Task SubscribeToTicker(string symbol)
    {
        string message = $"{{\"type\": \"subscribe\", \"channels\": [{{\"name\": \"ticker\", \"product_ids\": [\"{symbol}\"]}}]}}";
        await SendMessageAsync(message);
    }

    private async Task SendMessageAsync(string message)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        await _clientWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}
