using ENet;
using GamePackets;
using SnackGamePackServer.RoomSystem;
using SnackGamePackServer.ServerCore;

namespace GamePacketHandler
{
    public class PacketDispatcher
    {
        public Boolean Dispatch(Packet packet)
        {
            var gamePacket = GamePacketUtility.DeserializePacket(packet);

            if (gamePacket == null)
            {
                return false;
            }

            // 아직 룸 배정이 안 된 상태라면 디스패치하지 않고 메인 서버에서 직접 처리
            if (gamePacket.RoomIndex != null)
            {
                MainServer.Instance!.PostGamePacket(gamePacket);
                return false;
            }

            return RoomManager.DispatchGamePacket(gamePacket);
        }
    }
}
