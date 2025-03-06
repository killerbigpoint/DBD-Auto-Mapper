namespace DBD_Auto_Mappper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            WebSocketConnector websocket = new WebSocketConnector("localhost", 7788);
            websocket.Connect();
        }
    }
}
