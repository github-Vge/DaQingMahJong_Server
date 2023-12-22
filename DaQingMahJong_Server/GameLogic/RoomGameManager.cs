using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DaQingMahJong_Server.NetMessage.GameNetMessage;
using static DaQingMahJong_Server.NetMessage.NetMessage;
using VgeGameServer.Logic;
using VgeGameServer.Network;

namespace DaQingMahJong_Server.GameLogic;

/// <summary>
/// 管理RoomGame的类
/// </summary>
public static class RoomGameManager
{
    /// <summary>key:房间号，value:房间的一场游戏</summary>
    public static Dictionary<int, RoomGame> RoomGames { get; set; } = new Dictionary<int, RoomGame>();


    /// <summary>
    /// 新开一场游戏
    /// </summary>
    /// <param name="roomId"></param>
    public static void StartANewGame(int roomId)
    {
        //新增一场游戏
        RoomGames[roomId] = new RoomGame(roomId);

        //开始一局新游戏
        RoomMahJongTilesManager.StartANewGame(roomId);
    }

    /// <summary>
    /// 为所有玩家发初始的牌
    /// </summary>
    public static void InitTiles(int roomId)
    {
        //为每个玩家发牌
        for (int i = 1; i <= 4; i++)
        {
            RoomMahJongTilesManager.roomTiles[roomId].DealTiles(i, 13);
        }
        //为东家再发一张牌
        RoomMahJongTilesManager.roomTiles[roomId].DealTiles(1, 1);
        //构造消息
        Dictionary<int, List<MahJongType>> tiles = new Dictionary<int, List<MahJongType>>();
        for (int i = 1; i <= 4; i++)
        {
            tiles[i] = RoomMahJongTilesManager.roomTiles[roomId].mPlayerTiles[i].tiles;
        }
        //向当前房间玩家发送消息
        InitTiles_Handler.SendMessage(roomId, tiles);
    }


    /// <summary>
    /// 重新开始游戏
    /// </summary>
    /// <param name="roomId">房间Id</param>
    public static void RestartGame(int roomId)
    {
        //重新开始
        StartANewGame(roomId);
        //发牌
        InitTiles(roomId);
    }
}
