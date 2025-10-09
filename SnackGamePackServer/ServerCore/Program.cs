using ENet;

namespace SnackGamePackServer.ServerCore
{
    public static class Program
    {
        static void Main()
        {
            Library.Initialize();

            MainServer.StartServer();

            Library.Deinitialize();
        }
    }
}