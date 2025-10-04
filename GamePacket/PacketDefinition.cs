using MessagePack;

namespace GamePacket
{
    // @TODO: IPacketPayload ���� Ÿ�Ե� �߰��� ������ �� Ÿ�Կ� �ڵ����� �߰��ǵ��� ��ƿ�� ����
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