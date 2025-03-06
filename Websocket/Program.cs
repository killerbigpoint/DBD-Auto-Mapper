namespace DBD_Auto_Mapper_Websocket
{
    internal class Program
    {
        private static WebsocketListener server = null!;

        static void Main(string[] args)
        {
            server = new WebsocketListener();
            server.Initialize();
        }
    }
}
