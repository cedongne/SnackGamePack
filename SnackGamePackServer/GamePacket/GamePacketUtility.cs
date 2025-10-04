using MessagePack;
using ENet;

namespace GamePacket
{
    public class GamePacketUtility
    {
        private static Packet? CreatePacket(IPacketPayload payload)
        {
            var packet = default(Packet);
            var serializedPayload = GamePacket.Serialize(payload);

            if(serializedPayload == null)
            {
                return null;
            }

            packet.Create(MessagePackSerializer.Serialize(serializedPayload), PacketFlags.Reliable);

            return packet;
        }

        public static Boolean SendPacket(Peer client, IPacketPayload payload)
        {
            var wrappedPacket = CreatePacket(payload);

            if (wrappedPacket == null)
            {
                return false;
            }

            var packet = wrappedPacket.Value;

            return client.Send(0, ref packet);
        }

        public static Boolean ReceivePacket(Packet receivedPacket, out IPacketPayload? deserializedPayload)
        {
            // Packet 데이터 읽기
            var buffer = new byte[receivedPacket.Length];
            receivedPacket.CopyTo(buffer);

            var wrappedPacket = MessagePackSerializer.Deserialize<GamePacket>(buffer);

            if (wrappedPacket == null)
            {
                deserializedPayload = default;
                return false;
            }

            deserializedPayload = wrappedPacket.Deserialize();
            return true;
        }
    }
}