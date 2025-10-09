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

    public interface IGamePacketPayload;

    [MessagePackObject(true)]
    public record ReqConnectClientPacket : IGamePacketPayload
    {
        public String? UserId { get; init; }
    }

    [MessagePackObject(true)]
    public record AckConnectClientPacket : IGamePacketPayload
    {
        public required String UserId { get; init; }
    }
}