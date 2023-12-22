using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DaQingMahJong_Server.NetMessage.NetMessage;

namespace DaQingMahJong_Server.GameLogic
{
    public static class RoomMahJongTilesManager
    {
        /// <summary>麻将的初始牌，只读</summary>
        public static readonly List<MahJongType> mInitMahJongList = new List<MahJongType>();

        /// <summary>key:房间号，value:每个房间的麻将列表</summary>
        public static Dictionary<int, RoomMahJongTiles> roomTiles = new Dictionary<int, RoomMahJongTiles>();

        static RoomMahJongTilesManager()
        {
            //每种类型的麻将添加4张
            for (int i = 1; i < Enum.GetValues(typeof(MahJongType)).Length; i++)
            {
                for (int j = 0; j < 4; j++)//添加4张
                {
                    mInitMahJongList.Add((MahJongType)i);
                }
            }
        }

        /// <summary>
        /// 开始一局新的游戏，由RoomGameManager调用
        /// </summary>
        /// <param name="roomId">房间Id</param>
        public static void StartANewGame(int roomId)
        {
            //新建麻将牌的类
            RoomMahJongTiles roomMahJongTiles = new RoomMahJongTiles(roomId);
            //记录对应房间的麻将
            roomTiles[roomId] = roomMahJongTiles;
            
        }


    }



}
