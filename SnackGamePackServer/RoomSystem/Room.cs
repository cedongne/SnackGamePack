using ENet;
using GamePackets;
using System.Collections.Frozen;
using System.Threading.Channels;

namespace SnackGamePackServer.RoomSystem
{
    public abstract class Room : IWithGamePacketHandlerDefinition
    {
        /// <summary>
        /// 0이면 Lobby, 자연수면 커스텀 게임 룸
        /// </summary>
        public Int32 RoomIndex { get; private set; }

        // Room 단위로 단일 큐 기반 동기화
        private readonly Channel<Func<Boolean>> _channel;

        private readonly FrozenDictionary<GamePacketType, Func<Peer, IGamePacketPayload, Boolean>> _handlers;

        protected Room(Int32 roomIndex)
        {
            RoomIndex = roomIndex;

            UnboundedChannelOptions unboundedChannelOptions = new UnboundedChannelOptions();
            unboundedChannelOptions.SingleReader = true;

            // 일단 버퍼 사이즈 제한 없게 하는데, 필요하면 Bounded로 변경.
            _channel = Channel.CreateUnbounded<Func<Boolean>>(unboundedChannelOptions);
            _handlers = FrozenDictionary.ToFrozenDictionary(GamePacketHandlerDefinition);
        }

        public static Room Create<TRoomType>()
            where TRoomType : Room
        {
            return (TRoomType)Activator.CreateInstance(typeof(TRoomType), [])!;
        }

        public Boolean PostPacket(GamePacket gamePacket)
        {
            if (_handlers.TryGetValue(gamePacket.PacketType, out var handler) == false)
            {
                return false; 
            }

            return _channel.Writer.TryWrite(() => handler(gamePacket.Sender, gamePacket.Payload));
        }

        public abstract IReadOnlyList<KeyValuePair<GamePacketType, Func<Peer, IGamePacketPayload, bool>>> GamePacketHandlerDefinition { get; }
    }

    public sealed class Lobby : Room
    {
        private Lobby() : base(0) { }

        public override IReadOnlyList<KeyValuePair<GamePacketType, Func<Peer, IGamePacketPayload, bool>>> GamePacketHandlerDefinition
            => [new ()];
    }

    /// <summary>
    /// 게임 룸의 추상화 계층. 각 게임 종류에 맞게 이 타입을 상속 받아 처리할 로직을 기술함.
    /// </summary>
    public abstract class GameRoom : Room
    {
        protected GameRoom() : base(RoomManager.IssueNewRoomIndex()) { }
    }

    public sealed class ConcreteGameRoom : GameRoom
    {
        private ConcreteGameRoom() : base() { }

        public override IReadOnlyList<KeyValuePair<GamePacketType, Func<Peer, IGamePacketPayload, bool>>> GamePacketHandlerDefinition
            => [new()];
    }
}
