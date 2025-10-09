using ENet;
using GamePacketHandler;
using GamePackets;
using System.Collections.Frozen;
using System.Threading.Channels;

namespace SnackGamePackServer.ServerCore
{
    public class MainServer : IWithGamePacketHandlerDefinition
    {
        public static MainServer? Instance { get; private set; }

        private static Host _server = new Host();
        private static readonly PacketDispatcher _dispatcher = new PacketDispatcher();


        private readonly FrozenDictionary<GamePacketType, Func<Peer, IGamePacketPayload, Boolean>> _handlers;
        // 단일 큐 기반 동기화. 비동기 작업들까지 처리 가능함.
        private Channel<Func<IGamePacketPayload, Boolean>> _channel;

        private MainServer()
        {
            var channelOptions = new UnboundedChannelOptions();
            channelOptions.SingleReader = true;

            _channel = Channel.CreateUnbounded<Func<IGamePacketPayload, Boolean>>(channelOptions);
            _handlers = FrozenDictionary.ToFrozenDictionary(GamePacketHandlerDefinition);
        }

        public static void StartServer()
        {
            Instance = new MainServer();

            BindServer();
            ListenPacketEvent();

        }
        private static void BindServer()
        {
            var address = new Address();

            address.SetHost("127.0.0.1");
            address.Port = 1234;

            // 최대 접속 클라이언트 10명, 채널 2개
            _server.Create(address, 10, 2);

            Console.WriteLine("Server Started.");
        }

        /// <summary>
        /// <see cref="ENet.Host"/>를 통해 전달 받은 I/O 이벤트를 꺼내올 수 있도록 함.   
        /// </summary>
        private static void ListenPacketEvent()
        {
            while (true)
            {
                if (_server.Service(15, out Event netEvent) <= 0)
                {
                    // @TODO: 추후 다시 검토
                    // break;
                }

                switch (netEvent.Type)
                {
                    case EventType.Connect:
                        Console.WriteLine("Client connected - ID: " + netEvent.Peer.ID);
                        break;

                    case EventType.Receive:
                        _dispatcher.Dispatch(netEvent.Packet);
                        netEvent.Packet.Dispose();
                        break;

                    case EventType.Disconnect:
                        Console.WriteLine("Client disconnected - ID: " + netEvent.Peer.ID);
                        break;
                }
            }
        }

        public Boolean PostGamePacket(GamePacket gamePacket)
        {
            return _handlers.TryGetValue(gamePacket.PacketType, out var handler)
                && handler.Invoke(gamePacket.Sender, gamePacket.Payload);
        }

        public IReadOnlyList<KeyValuePair<GamePacketType, Func<Peer, IGamePacketPayload, bool>>> GamePacketHandlerDefinition
            => [
                    new (GamePacketType.ReqConnectClientPacket, (sender, packetPayload) =>
                    {
                        if (packetPayload is not ReqConnectClientPacket req)
                        {
                            return false;
                        }

                        var clientId = req.UserId ?? UserAccount.IssueDummyClientId();
                        Console.WriteLine($"Client'{clientId}' is connected.");

                        // @TODO: Sender를 ReqConnectClientPacket을 보낸 클라이언트로 설정해두고 있는데
                        // 이후 GameObject가 Peer 개념을 포함하는 등이 되면 다시 구조화
                        sender.SendPacket(sender, new AckConnectClientPacket
                        {
                            UserId = clientId
                        });

                        return true;
                    })
               ];

    }
}