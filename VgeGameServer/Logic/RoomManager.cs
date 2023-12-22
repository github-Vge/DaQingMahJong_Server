

using VgeGameServer.Network;

namespace VgeGameServer.Logic;

public static class RoomManager
{
    /// <summary>房间列表</summary>
    public static Dictionary<int, Room> roomDict = new Dictionary<int, Room>();

    /// <summary>
    /// 创建房间
    /// </summary>
    /// <returns>房间对象</returns>
    public static Room AddRoom()
    {
        int newRoomID = 0;
        for (int i = 1; i < int.MaxValue; i++)
        {
            if (!roomDict.ContainsKey(i))
            {
                newRoomID = i;
                break;
            }
        }

        Room room = new Room();
        room.roomID = newRoomID;
        roomDict.Add(room.roomID, room);
        return room;
    }

    /// <summary>
    /// 获取可用房间，可用房间是指房间存在且玩家没满
    /// </summary>
    /// <returns>可用房间的Id，如果没有返回null</returns>
    public static Room? GetAvailableRoom()
    {
        //遍历所有房间
        foreach (Room room in roomDict.Values)
        {
            if(room.playerList.Count != Room.maxPlayer)//如果房间人数没满
            {
                //返回房间
                return room;
            }
        }
        return null;
    }


    /// <summary>
    /// 根据房间ID获取到房间对象
    /// </summary>
    /// <param name="roomID">房间ID</param>
    /// <returns>Room对象。如果为null，则房间不存在</returns>
    public static Room? GetRoom(int roomID)
    {
        return roomDict.Values.FirstOrDefault(t => t.roomID == roomID);
    }

    /// <summary>
    /// 玩家离开所在房间时调用
    /// </summary>
    /// <param name="playerId">离开玩家的ID</param>
    public static void PlayerLeave(string playerId)
    {
        //获取房间
        Room room = roomDict.Values.First(t => t.roomID == PlayerManager.GetPlayer(playerId).roomID);
        //将玩家移出房间
        room.RemovePlayer(playerId);
        if (room.playerList.Count == 0)//房间里没有玩家了
        {
            //移除房间
            roomDict.Remove(room.roomID);
        }
        //玩家离开房间事件[!重要代码]
        NetMessageBase.CallPlayerLeaveEvent(PlayerManager.GetPlayer(playerId).clientState);
    }

    /// <summary>
    /// 玩家做好重新开始游戏的准备时调用
    /// </summary>
    /// <param name="playerID">玩家ID</param>
    //public static void PlayerRestartGame(string playerID)
    //{
    //    Room room = roomDict.Values.First(t => t.roomID == PlayerManager.GetPlayer(playerID).roomID);
    //    room.PlayerRestartGame(playerID);
    //}


}
