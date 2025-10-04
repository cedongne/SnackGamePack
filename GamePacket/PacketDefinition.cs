using MessagePack;

namespace GamePacket
{
    // @TODO: IPacketPayload 하위 타입들 추가될 때마다 이 타입에 자동으로 추가되도록 유틸성 제공
    public enum GamePacketType
    {
        TestPacket
    }

    public interface IPacketPayload;

    [MessagePackObject]
    public record TestPacket : IPacketPayload
    {
        Int32 Member { get; init; }
    }
}