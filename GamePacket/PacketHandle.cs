using MessagePack;

namespace GamePacket
{
    /// <summary>
    /// �ݵ�� <seealso cref="CreatePacket"/> ���丮 �޼��带 ���ؼ��� ȣ��. MessagePack Ư�� �� public constructor�� ���� �� ����.
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