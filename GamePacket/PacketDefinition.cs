using ENet;
using MessagePack;

namespace GamePackets
{
    // @TODO: IPacketPayload 하위 타입들 추가될 때마다 이 타입에 자동으로 추가되도록 유틸성 제공
    public enum GamePacketType
    {
        Invalid,
        ReqConnectClientPacket,
        AckConnectClientPacket
    }

    public interface IGamePacketPayload
    {
        Peer Sender { get; init; }
        Int32 ReceiveRoomId { get; init; }
    }

    [MessagePackObject(true)]
    public record ReqConnectClientPacket : IGamePacketPayload
    {
        public Peer Sender { get; init; }
        public Int32 ReceiveRoomId { get; init; }
        public String? UserId { get; init; }
    }

    [MessagePackObject(true)]
    public record AckConnectClientPacket : IGamePacketPayload
    {
        public Peer Sender { get; init; }
        public Int32 ReceiveRoomId { get; init; }
        public required String UserId { get; init; }
    }
}