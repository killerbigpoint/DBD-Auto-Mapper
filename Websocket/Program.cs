namespace DBD_Auto_Mapper_Websocket
{
    internal class Program
    {
        private static WebSocketListener server = null!;

        static void Main(string[] args)
        {
            server = new WebSocketListener(7788);
            server.Initialize();

            while (server.IsRunning() == true)
            {
                Console.ReadKey();
            }
        }
    }
}
