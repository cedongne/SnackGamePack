using ENet;
using GamePackets;
using System.Collections.Frozen;
using System.Threading.Channels;

namespace SnackGamePackServer.GameRoom
{
    public interface IWithGamePacketHandlerDefinition
    {
        IReadOnlyList<KeyValuePair<GamePacketType, Func<IGamePacketPayload, Boolean>>> GamePacketHandlerDefinition { get; }
    }
    public abstract class Room : IWithGamePacketHandlerDefinition
    {
        /// <summary>
        /// 0이면 Lobby, 자연수이면 커스텀 게임 룸
        /// </summary>
        public Int32 RoomIndex { get; private set; }

        // Room 단위로 단일 큐 기반 동기화
        private readonly Channel<Func<Task>> _channel;

        private readonly FrozenDictionary<GamePacketType, Func<IGamePacketPayload, Boolean>> _handlers;

        protected Room(Int32 roomIndex)
        {
            RoomIndex = roomIndex;

            UnboundedChannelOptions unboundedChannelOptions = new UnboundedChannelOptions();
            unboundedChannelOptions.SingleReader = true;

            // 일단 버퍼 사이즈 제한 없게 하는데, 필요하면 Bounded로 변경.
            _channel = Channel.CreateUnbounded<Func<Task>>(unboundedChannelOptions);
            _handlers = FrozenDictionary.ToFrozenDictionary(GamePacketHandlerDefinition);
        }

        public static Room? Create()
        {
            return new Room(LatestRoomIndex++);
        }

        public Boolean PostPacket(Peer sender, SerializedGamePacket gamePacket)
        {
            _channel.Writer.TryWrite()
        }

        public abstract IReadOnlyList<KeyValuePair<GamePacketType, Func<IGamePacketPayload, bool>>> GamePacketHandlerDefinition { get; }
    }

    public sealed class Lobby : Room
    {
        protected Lobby(Int32 roomIndex) : base(roomIndex) { }

        public override IReadOnlyList<KeyValuePair<GamePacketType, Func<IGamePacketPayload, bool>>> GamePacketHandlerDefinition
            => [new ()];
    }

    /// <summary>
    /// 게임 룸의 추상화 계층. 각 게임 종류에 맞게 이 타입을 상속 받아 처리할 로직을 기술함.
    /// </summary>
    public abstract class GameRoom : Room
    {
        protected GameRoom(Int32 roomIndex) : base(roomIndex)
        {
        }
    }

    public sealed class ConcreteGameRoom : GameRoom
    {
        protected ConcreteGameRoom(Int32 roomIndex) : base(roomIndex)
        {
        }
    }
}
