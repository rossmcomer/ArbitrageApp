using System.Net.WebSockets;
using System.Text;

public class BinanceWebSocketService : WebSocketService
{
    public BinanceWebSocketService() : base("wss://stream.binance.us:9443/ws")
    {
    }

    public async Task SubscribeToTicker(string symbol)
    {
        string message = $"{{\"method\": \"SUBSCRIBE\", \"params\": [\"{symbol}@trade\"], \"id\": 1}}";
        await SendMessageAsync(message);
    }

    private async Task SendMessageAsync(string message)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        await _clientWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}
