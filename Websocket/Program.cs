namespace DBD_Auto_Mapper_Websocket
{
    internal class Program
    {
        private static WebsocketListener server = null!;

        static void Main(string[] args)
        {
            server = new WebsocketListener(7788);
            server.Initialize();

            while (server.IsRunning() == true)
            {
                Console.ReadKey();
            }
        }
    }
}
