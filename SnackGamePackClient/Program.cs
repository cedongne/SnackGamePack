using ENet;

public static class MainServer
{
    private static Host _client = new Host();
    private static String? userId;

    private static Peer _serverPeer;
    private static String _serverIp = "127.0.0.1";
    private static ushort _serverPort = 1234;

    static void Main()
    {
        Library.Initialize();

        userId = Console.ReadLine();

        var serverAddress = new Address { Port = _serverPort };
        serverAddress.SetIP(_serverIp);
        
        _serverPeer = _client.Connect(serverAddress, 2);
        OpenPacketHandler();

        Library.Deinitialize();
    }

    private static void OpenPacketHandler()
    {
        Event netEvent;

        while (true)
        {
            if (_client.Service(15, out netEvent) <= 0)
            {
                break;
            }

            switch (netEvent.Type)
            {
                case EventType.Connect:
                    Console.WriteLine($"[{userId ?? "<DummyClient>"}] Connecting to server...");
                    break;

                case EventType.Receive:
                    GamePacketUtility.ReceivePacket(netEvent.Packet, out var deserializedPayload);
                    // @TODO: deserializedPayload는 이를 처리할 스레드를 결정해야 하는데,
                    // Dispatch 전용 스레드를 만들어 두고, 게임룸 또는 다른 스레드로 보내도록 설계.
                    // 게임룸이 생길 때 스레드를 할당하고 이 안에선 싱글 스레드 동작을 보장함.
                    netEvent.Packet.Dispose();
                    break;

                case EventType.Disconnect:
                    Console.WriteLine("Client disconnected - ID: " + netEvent.Peer.ID);
                    break;
            }
        }
    }
}