using ENet;
using GamePackets;
using SnackGamePackServer.GameRoom;
using SnackGamePackServer.ServerCore;
using System.Collections.Frozen;

namespace SnackGamePackServer.GamePacketHandler
{
    public class PacketDispatcher : IWithGamePacketHandlerDefinition
    {
        public PacketDispatcher() 
        {
            _handlers = FrozenDictionary.ToFrozenDictionary(GamePacketHandlerDefinition);
        }

        private readonly FrozenDictionary<GamePacketType, Func<IGamePacketPayload, Boolean>> _handlers;

        public Boolean Dispatch(Packet packet)
        {
            var gamePacket = GamePacketUtility.DeserializePacket(packet);

            if(gamePacket == null)
            {
                return false;
            }

            // 아직 룸 배정이 안 된 상태라면 디스패치하지 않고 메인 서버에서 직접 처리
            if(gamePacket.RoomUid != null)
            {

            }

            // @TODO: Packet을 처리할 스레드를 결정해야 하는데, 현재는 하나로만 처리하도록 러프하게 구성.
            // Dispatch 전용 스레드를 만들어 두고 게임룸 또는 다른 스레드로 보내도록 설계.
            // 게임룸이 생길 때 스레드를 할당하고 이 안에선 싱글 스레드 동작을 보장함.
            if (_handlers.TryGetValue(gamePacket.PacketType, out var handler))
            {
                handler.Invoke(payload);
                return true;
            }

            return false;
        }

        public IReadOnlyList<KeyValuePair<GamePacketType, Func<IGamePacketPayload, bool>>> GamePacketHandlerDefinition
            => [
                    new (GamePacketType.ReqConnectClientPacket, packetPayload =>
                    {
                        if(packetPayload is not ReqConnectClientPacket req)
                        {
                            return false;
                        }

                        var clientId = req.UserId ?? UserAccount.IssueDummyClientId();
                        Console.WriteLine($"Client'{clientId}' is connected.");

                        req.SendPacket()
                        return true;
                    }),
                    new (GamePacketType.AckConnectClientPacket, packetPayload =>
                    {
                        return true;
                    })
               ];

    }
}
