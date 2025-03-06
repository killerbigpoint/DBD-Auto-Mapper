using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace DBD_Auto_Mapper_Websocket
{
    class WebsocketListener
    {
        private readonly HttpListener listener;
        private bool shouldRun;

        private int count = 0;

        public WebsocketListener(int port)
        {
            shouldRun = false;

            listener = new HttpListener();
            listener.Prefixes.Add($"http://+:{port}/game/");

            LogServer($"Initialized with port {port}");
        }

        public bool IsRunning()
        {
            return shouldRun;
        }

        public void Initialize()
        {
            LogServer("Started WebSocket");

            shouldRun = true;

            listener.Start();
            Listen();
        }

        public void Stop()
        {
            LogServer("Stopped WebSocket");

            shouldRun = false;
        }

        private async void Listen()
        {
            while (shouldRun == true)
            {
                HttpListenerContext listenerContext = await listener.GetContextAsync();
                if (listenerContext.Request.IsWebSocketRequest)
                {
                    ProcessRequest(listenerContext);
                }
                else
                {
                    listenerContext.Response.StatusCode = 400;
                    listenerContext.Response.Close();
                }
            }
        }

        private async void ProcessRequest(HttpListenerContext listenerContext)
        {
            WebSocketContext webSocketContext;

            try
            {
                webSocketContext = await listenerContext.AcceptWebSocketAsync(subProtocol: null);
                Interlocked.Increment(ref count);

                LogServer($"Processed: {count}");
            }
            catch (Exception e)
            {
                listenerContext.Response.StatusCode = 500;
                listenerContext.Response.Close();

                LogServer($"Exception: {e}");
                return;
            }

            WebSocket webSocket = webSocketContext.WebSocket;
            ProcessClient(webSocket);
        }

        private async void ProcessClient(WebSocket webSocket)
        {
            // Receive buffer
            byte[] receiveBuffer = new byte[1024];

            try
            {
                // While the WebSocket connection remains open run a simple loop that receives data and sends it back.
                while (webSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);

                        continue;
                    }
                    else if (receiveResult.MessageType == WebSocketMessageType.Text)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Cannot accept text frame", CancellationToken.None);

                        continue;
                    }

                    //await webSocket.SendAsync(new ArraySegment<byte>(receiveBuffer, 0, receiveResult.Count), WebSocketMessageType.Binary, receiveResult.EndOfMessage, CancellationToken.None);

                    string text = Encoding.ASCII.GetString(receiveBuffer, 0, receiveResult.Count);
                    LogGame($"Received -> {text}");
                }
            }
            catch (Exception e)
            {
                // Just log any exceptions to the console. Pretty much any exception that occurs when calling `SendAsync`/`ReceiveAsync`/`CloseAsync` is unrecoverable in that it will abort the connection and leave the `WebSocket` instance in an unusable state.
                LogServer($"Exception: {e}");
            }
            finally
            {
                // Clean up by disposing the WebSocket once it is closed/aborted.
                webSocket?.Dispose();
            }
        }

        // ----- Helpers ----- \\

        private void LogServer(string text)
        {
            Console.WriteLine($"[Server] {text}");
        }

        private void LogGame(string text)
        {
            Console.WriteLine($"[Game] {text}");
        }
    }
}
