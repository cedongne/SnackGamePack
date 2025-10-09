using MessagePack;
using GamePackets;
using ENet;

namespace SnackGamePackServer.GamePacketHandler
{
    public static class GamePacketUtility
    {
        private static Packet? CreatePacket(Peer sender, IGamePacketPayload payload)
        {
            var packet = default(Packet);
            var gamePacket = SerializedGamePacket.Create(sender, payload);

            if(gamePacket == null)
            {
                return null;
            }

            packet.Create(MessagePackSerializer.Serialize(gamePacket), PacketFlags.Reliable);

            return packet;
        }

        public static Packet? SerializePacket(Peer sender, IGamePacketPayload payload)
        {
            var wrappedPacket = CreatePacket(sender, payload);

            if (wrappedPacket == null)
            {
                return default;
            }

            var packet = wrappedPacket.Value;

            return packet;
        }

        public static Boolean SendPacket(this Peer receiver, Peer sender, IGamePacketPayload payload)
        {
            var serializedPacket = SerializePacket(sender, payload);

            if (serializedPacket == null)
            {
                return false;
            }

            var value = serializedPacket.Value;

            return receiver.Send(0, ref value);
        }

        public static SerializedGamePacket? DeserializePacket(Packet receivedPacket)
        {
            // Packet 데이터 읽기
            var buffer = new byte[receivedPacket.Length];
            receivedPacket.CopyTo(buffer);

            return MessagePackSerializer.Deserialize<SerializedGamePacket>(buffer);
        }
    }
}