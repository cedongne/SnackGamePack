using ENet;
using GamePacket;

namespace SnackGamePackServer.ServerCore
{
    public static partial class MainServer
    {
        private static Host _server = new Host();

        private static void BindServer()
        {
            var address = new Address();

            address.SetHost("127.0.0.1");
            address.Port = 1234;

            // 최대 접속 클라이언트 10명, 채널 2개
            _server.Create(address, 10, 2);

            Console.WriteLine("Server Started.");
        }

        private static void OpenPacketHandler()
        {
            Event netEvent;

            while (true)
            {
                var isPolled = false;

                while (!isPolled)
                {
                    if (_server.Service(15, out netEvent) <= 0)
                    {
                        break;
                    }

                    switch (netEvent.Type)
                    {
                        case EventType.Connect:
                            Console.WriteLine("Client connected - ID: " + netEvent.Peer.ID);
                            break;

                        case EventType.Receive:
                            GamePacketUtility.ReceivePacket(netEvent.Packet, out var deserializedPayload);
                            netEvent.Packet.Dispose();
                            break;

                        case EventType.Disconnect:
                            Console.WriteLine("Client disconnected - ID: " + netEvent.Peer.ID);
                            break;
                    }
                    isPolled = true;
                }
            }
        }
    }
}