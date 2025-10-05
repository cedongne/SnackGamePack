using ENet;
using GamePackets;
using SnackGamePackServer.GamePacketHandler;
using System.Collections.Frozen;
using System.Threading.Channels;

namespace SnackGamePackServer.ServerCore
{
    public static partial class MainServer
    {
        private static Host _server = new Host();
        private static readonly PacketDispatcher _dispatcher = new PacketDispatcher();

        // Room 단위로 단일 큐 기반 동기화. 비동기 작업들까지 처리 가능함.
        private static Channel<Func<IGamePacketPayload, Boolean>> _channel;

        private static void Initialize()
        {
            var channelOptions = new UnboundedChannelOptions();
            channelOptions.SingleReader = true;

            _channel = Channel.CreateUnbounded<Func<IGamePacketPayload, Boolean>>(channelOptions);

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
            Event netEvent;

            while (true)
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
                        _dispatcher.Dispatch(netEvent.Packet);
                        netEvent.Packet.Dispose();
                        break;

                    case EventType.Disconnect:
                        Console.WriteLine("Client disconnected - ID: " + netEvent.Peer.ID);
                        break;
                }
            }
        }

        public static Boolean PostPacket(Peer sender, GamePacket gamePacket)
        {
            if(GamePacketHandlersDefinition.TryGetValue(gamePacket.PacketType, out var handler) == false)
            {
                return false;
            }
            _channel.Writer.TryWrite(handler);

            return true;
        }

        private static Func<IGamePacketPayload, Boolean> GetGamePacketHandler(GamePacket gamePacket) => _ =>
        {
            if (GamePacketHandlersDefinition.TryGetValue(gamePacket.PacketType, out var handler) == false)
            {
                return ;
            }

            return handler;
        };

        private static FrozenDictionary<GamePacketType, Func<IGamePacketPayload, Boolean>> GamePacketHandlersDefinition
            => FrozenDictionary.ToFrozenDictionary(new List<KeyValuePair<GamePacketType, Func<IGamePacketPayload, Boolean>>>
            {
                new (GamePacketType.ReqConnectClientPacket, packetPayload =>
                {
                    if(packetPayload is not ReqConnectClientPacket req)
                    {
                        return false;
                    }

                    var clientId = req.UserId ?? UserAccount.IssueDummyClientId();
                    Console.WriteLine($"Client'{clientId}' is connected.");

                    req.SendPacket()
                    return true;
                }),
                new (GamePacketType.AckConnectClientPacket, packetPayload =>
                {
                    return true;
                })
            });
    }
}