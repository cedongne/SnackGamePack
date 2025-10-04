using MessagePack;

namespace GamePacket
{
    /// <summary>
    /// 반드시 <seealso cref="CreatePacket"/> 팩토리 메서드를 통해서만 호출. MessagePack 특성 상 public constructor를 막을 수 없음.
    /// </summary>
    [MessagePackObject(true)]
    public class GamePacket
    {
        public GamePacketType PacketType { get; init; }
        public required Byte[] Payload { get; init; }

        public static GamePacket? Serialize(IPacketPayload payload)
        {
            if (Enum.TryParse<GamePacketType>(String.Intern(payload.GetType().Name), out var packetType) == false)
            {
                return default;
            }

            return new GamePacket
            {
                PacketType = packetType,
                Payload = MessagePackSerializer.Serialize(payload)
            };
        }

        public IPacketPayload Deserialize()
        {
            return PacketType switch
            {
                GamePacketType.TestPacket => MessagePackSerializer.Deserialize<TestPacket>(Payload),
                _ => throw new InvalidOperationException($"{nameof(GamePacket)}.{nameof(Deserialize)}: {nameof(PacketType)}'{PacketType.ToString()}' is invalid.")
            };
        }
    }
}