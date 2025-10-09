using ENet;
using MessagePack;

namespace GamePackets
{
    /// <summary>
    /// 네트워크로 실제 전달할 목적으로 Payload인 <see cref="IGamePacketPayload"/>를 Serialize 한 상태의 패킷. Payload를 실제로 참조하고자 한다면 <see cref="GamePacket"/>을 사용. <br/>
    /// 반드시 <seealso cref="CreatePacket"/> 팩토리 메서드를 통해서만 호출. MessagePack 특성 상 public constructor를 막을 수 없음.
    /// </summary>
    [MessagePackObject(true)]
    public class SerializedGamePacket
    {
        public GamePacketType PacketType { get; init; }
        public Peer Sender { get; init; }
        public Int32? RoomUid { get; init; }
        /// <summary>
        /// <see cref="IGamePacketPayload"/> 타입을 Serialize 한 정보. MessagePack에는 추상 타입을 담았을 때 구체 타입으로 Deserialize 하기 어려워서<br/>
        /// 먼저 Serilaize 한 후 담아야 함.
        /// </summary>
        public required Byte[] Payload { get; init; }

        public static SerializedGamePacket? Create(Peer sender, IGamePacketPayload payload)
        {
            if (Enum.TryParse<GamePacketType>(String.Intern(payload.GetType().Name), out var packetType) == false)
            {
                return default;
            }

            return new SerializedGamePacket
            {
                Sender = sender,
                PacketType = packetType,
                Payload = MessagePackSerializer.Serialize(payload)
            };
        }

        public IGamePacketPayload GetPayload()
        {
            return PacketType switch
            {
                GamePacketType.ReqConnectClientPacket => MessagePackSerializer.Deserialize<ReqConnectClientPacket>(Payload),
                GamePacketType.AckConnectClientPacket => MessagePackSerializer.Deserialize<AckConnectClientPacket>(Payload),
                _ => throw new InvalidOperationException($"{nameof(SerializedGamePacket)}.{nameof(GetPayload)}: {nameof(PacketType)}'{PacketType.ToString()}' is invalid.")
            };
        }
    }

    public class GamePacket
    {
        public GamePacketType PacketType { get; init; }
        public Peer Sender { get; init; }
        public Int32? RoomIndex { get; init; }
        public required IGamePacketPayload Payload { get; init; }

        private GamePacket() { }

        public static GamePacket? Create(SerializedGamePacket serializedPacket)
        {
            var payload = serializedPacket.GetPayload();
            if (payload is null)
            {
                return default;
            }
            return new GamePacket
            {
                PacketType = serializedPacket.PacketType,
                Sender = serializedPacket.Sender,
                RoomIndex = serializedPacket.RoomUid,
                Payload = payload
            };
        }
    }

    public interface IWithGamePacketHandlerDefinition
    {
        /// <summary>
        /// Peer: Sender
        /// </summary>
        IReadOnlyList<KeyValuePair<GamePacketType, Func<Peer, IGamePacketPayload, Boolean>>> GamePacketHandlerDefinition { get; }
    }
}