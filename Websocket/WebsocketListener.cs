using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace DBD_Auto_Mapper_Websocket
{
    class WebSocketListener
    {
        private readonly HttpListener listener;
        private bool shouldRun;

        public WebSocketListener(int port)
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
            }
            catch (Exception e)
            {
                listenerContext.Response.StatusCode = 500;
                listenerContext.Response.Close();

                LogServer($"Exception: {e}");
                return;
            }

            WebSocket webSocket = webSocketContext.WebSocket;
            ProcessClient(listenerContext, webSocket);
        }

        private async void ProcessClient(HttpListenerContext listenerContext, WebSocket webSocket)
        {
            LogServer($"Client connected -> {listenerContext.Request.UserHostAddress}");

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
                    LogClient($"Received -> {text}");
                }
            }
            catch (WebSocketException e)
            {
                if (e.InnerException is HttpListenerException ex && ex.ErrorCode == 995)
                {
                    LogServer($"Client closed websocket");
                }
                else
                {
                    LogServer($"Http Exception: {e.Message}");
                }
            }
            catch (Exception e)
            {
                // Just log any exceptions to the console. Pretty much any exception that occurs when calling `SendAsync`/`ReceiveAsync`/`CloseAsync` is unrecoverable in that it will abort the connection and leave the `WebSocket` instance in an unusable state.
                LogServer($"General Exception: {e}");
            }
            finally
            {
                // Clean up by disposing the WebSocket once it is closed/aborted.
                webSocket?.Dispose();
            }

            LogServer($"Client disconnected -> {listenerContext.Request.UserHostAddress}");
        }

        // ----- Helpers ----- \\

        private static void LogServer(string text)
        {
            Console.WriteLine($"[Server] {text}");
        }

        private static void LogClient(string text)
        {
            Console.WriteLine($"[Client] {text}");
        }
    }
}
