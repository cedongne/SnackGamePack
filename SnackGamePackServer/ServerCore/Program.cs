using ENet;

namespace SnackGamePackServer.ServerCore
{
    public static partial class MainServer
    {
        static void Main()
        {
            Library.Initialize();

            BindServer();
            ListenPacketEvent();

            Library.Deinitialize();
        }
    }
}