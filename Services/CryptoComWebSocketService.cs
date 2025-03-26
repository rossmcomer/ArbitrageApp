using System.Net.WebSockets;
using System.Text;

public class CryptoComWebSocketService : WebSocketService
{
    public CryptoComWebSocketService() : base("wss://stream.crypto.com/v2/market")
    {
    }

    public async Task SubscribeToTicker(string symbol)
    {
        string message = $"{{\"method\": \"subscribe\", \"params\": {{\"channels\": [\"ticker_{symbol}_usdt\"]}}}}";
        await SendMessageAsync(message);
    }

    private async Task SendMessageAsync(string message)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        await _clientWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}
