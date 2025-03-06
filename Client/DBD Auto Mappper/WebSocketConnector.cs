using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;

namespace DBD_Auto_Mappper
{
    class WebSocketConnector
    {
        private ClientWebSocket webSocket = null!;
        private readonly string url;

        //private byte[] sendBuffer = new byte[1024];

        public WebSocketConnector(string address, int port)
        {
            url = $"ws://{address}:{port}/game";

            LogServer($"Initialized with {address}:{port}");
        }

        public async void Connect()
        {
            LogServer($"Connecting");

            try
            {
                webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(new Uri(url), CancellationToken.None);

                LogServer($"Connected");

                await Receive();
            }
            catch (Exception ex)
            {
                LogServer($"Exception: {ex}");
            }
            finally
            {
                webSocket?.Dispose();
                LogServer("WebSocket closed");
            }
        }

        private async void Send(string text)
        {
            if (webSocket.State != WebSocketState.Open)
            {
                return;
            }

            byte[] sendBuffer = Encoding.ASCII.GetBytes(text);

            try
            {
                await webSocket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Binary, false, CancellationToken.None);
            }
            catch (Exception ex)
            {
                LogServer($"Exception: {ex}");
            }
        }

        private async Task Receive()
        {
            byte[] receiveBuffer = new byte[1024];
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    continue;
                }

                string text = Encoding.ASCII.GetString(receiveBuffer, 0, result.Count);
                LogClient($"Received -> {text}");
            }
        }

        // ----- Helpers ----- \\

        private static void LogServer(string text)
        {
            Debug.WriteLine($"[Server] {text}");
        }

        private static void LogClient(string text)
        {
            Debug.WriteLine($"[Game] {text}");
        }
    }
}
