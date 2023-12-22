using DaQingMahJong_Server.NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VgeGameServer.Logic;
using static DaQingMahJong_Server.NetMessage.GameNetMessage;
using static DaQingMahJong_Server.NetMessage.NetMessage;

namespace DaQingMahJong_Server.GameLogic;

/// <summary>
/// 一个房间内游戏的管理类，一个房间对应一个RoomGame
/// </summary>
public class RoomGame
{
    /// <summary>房间Id</summary>
    public int RoomId { get; private set; }

    /// <summary>所有可以吃牌玩家的操作回调。吃牌则whetherToEat为true，不吃牌为false</summary>
    public List<EatTileOperation> TotalNumberOfOperationsCurrentlyInProgress { get; set; }
        = new List<EatTileOperation>();

    /// <summary>四个玩家的实例</summary>
    public Dictionary<int, GamePlayer> Players { get; set; }

    public RoomGame(int roomId)
    {
        //记录房间Id
        RoomId = roomId;
        //新建四个玩家实例
        Players = new Dictionary<int, GamePlayer>();
        for (int i = 1; i <= 4; i++)
        {
            Players[i] = new GamePlayer();
            Players[i].InitPlayer(roomId, i, !RoomManager.roomDict[roomId].IsPlayerExist(i), i == 1);
        }
    }

    /// <summary>
    /// 玩家正在打出一张牌，先判断胡牌和吃牌[!重要代码]
    /// </summary>
    public void PlayerPlayingATile(int playerId, MahJongType mahJongType)
    {
        //胡牌判断
        foreach (GamePlayer player in Players.Values)
        {
            //新建胡牌操作数据
            WinOperationData winOperationData = new WinOperationData();
            winOperationData.mahJongType = mahJongType;
            if (player.PlayerId != playerId && player.CheckWin(ref winOperationData) == true)
            {
                //暂存胡牌数据
                GameNetMessage.WinOperation_Handler.GameOver = new GameNetMessage.GameOver
                {
                    overType = GameNetMessage.OverType.OtherPlayed,
                    winPlayerId = player.PlayerId,
                    fromPlayerId = playerId,
                    mahJongType = mahJongType,
                };
                //设置赢牌操作数据
                winOperationData.fromPlayerId = playerId;
                winOperationData.toPlayerId = player.PlayerId;
                //其他玩家胡牌操作
                GameNetMessage.WinOperation_Handler.SendMessage(RoomId, player.PlayerId, winOperationData);

                return;
            }
        }
        //吃牌判断。如果有其他玩家吃牌，则跳转到其他玩家的吃牌回合
        if (EatTileManager.CheckEat(RoomId, playerId, mahJongType) == true)
        {
            //其他玩家吃牌回合
            return;
        }
        //玩家打出了牌
        PlayerPlayedATile(playerId, mahJongType);
    }

    /// <summary>
    /// 玩家打出了一张牌[!重要代码]
    /// </summary>
    public void PlayerPlayedATile(int playerId, MahJongType mahJongType)
    {
        //下一个玩家行动
        int nextPlayerId = playerId % 4 + 1;
        //给玩家发一张牌
        Players[nextPlayerId].DealATile();
    }






}
