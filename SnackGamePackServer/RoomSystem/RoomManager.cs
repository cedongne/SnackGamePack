using GamePackets;

namespace SnackGamePackServer.RoomSystem
{
    public static class RoomManager
    {
        private static Int32 LatestRoomIndex { get; set; } = 0;

        private const Int32 LobbyRoomIndex = 0;

        private readonly static Dictionary<Int32, Room> _rooms = [];

        public static Int32 IssueNewRoomIndex() => LatestRoomIndex++;

        public static Room CreateLobby()
        {
            if(_rooms.TryGetValue(LobbyRoomIndex, out var lobbyRoom) == false)
            {
                lobbyRoom = Room.Create<Lobby>();
                _rooms.TryAdd(LobbyRoomIndex, lobbyRoom);
            }

            return lobbyRoom;
        }

        public static Room CreateNewGameRoom<TGameRoom>(out Int32 newRoomIndex)
            where TGameRoom : GameRoom
        {
            var newRoom = Room.Create<TGameRoom>();
            newRoomIndex = newRoom.RoomIndex;

            _rooms.TryAdd(newRoomIndex, newRoom);

            return newRoom;
        }

        public static Boolean DispatchGamePacket(GamePacket gamePacket)
        {
            var roomIndex = gamePacket.RoomIndex;

            if(roomIndex == null || gamePacket == null 
                || _rooms.TryGetValue(roomIndex.Value, out var targetRoom) == false)
            {
                return false;
            }

            return targetRoom.PostPacket(gamePacket);
        }
    }
}
