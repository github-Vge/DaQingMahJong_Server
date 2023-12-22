using DaQingMahJong_Server.GameLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static DaQingMahJong_Server.NetMessage.NetMessage;
using VgeGameServer.Logic;
using DaQingMahJong_Server.NetMessage;
using System.Numerics;
using static DaQingMahJong_Server.NetMessage.GameNetMessage;

/// <summary>
/// 游戏玩家，一个房间有4个GamePlayer
/// </summary>
public class GamePlayer
{
    /// <summary>玩家所在的房间Id</summary>
    public int RoomId { get; protected set; }
    /// <summary>玩家Id</summary>
    public int PlayerId { get; protected set; }
    /// <summary>玩家的状态</summary>
    public PlayerState State { get; protected set; }
    /// <summary>玩家是否听牌</summary>
    public bool IsListening { get; protected set; }
    /// <summary>玩家的听牌数据，仅当IsListening为true时有效</summary>
    public ListeningTilesData ListeningTiles { get; set; }
    /// <summary>是否是AI玩家</summary>
    public bool IsAIPlayer {  get; protected set; }
    /// <summary>是否是东家</summary>
    public bool IsHost { get; protected set; }

    /// <summary>
    /// 初始化玩家
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="host">是否是东家</param>
    public virtual void InitPlayer(int roomId, int playerId, bool aiPlayer, bool host)
    {
        //记录房间Id
        RoomId = roomId;
        //记录玩家Id
        PlayerId = playerId;
        //记录是否是AI玩家
        IsAIPlayer = aiPlayer;
        //记录东家信息
        IsHost = host;
        //未听牌
        IsListening = false;
        //设置玩家状态
        if (host == true)//如果是东家
        {
            //设置状态
            PlayingTile();
        }
    }


    /// <summary>
    /// 为玩家发一张牌
    /// </summary>
    public virtual void DealATile()
    {
        //给玩家发一张牌
        MahJongType mahJongType = RoomMahJongTilesManager.roomTiles[RoomId].DealATile(PlayerId);
        //没牌了，游戏结束
        if (mahJongType == default) return;

        //发送消息给玩家：发了一张牌
        GameNetMessage.DealATileToPlayer_Handler.SendMessage(RoomId, PlayerId, mahJongType);

        //新建胡牌操作数据
        WinOperationData winOperationData = new WinOperationData();
        winOperationData.mahJongType = mahJongType;
        //判断玩家是否胡牌（自摸）
        if (CheckWin(ref winOperationData, selfTouch: true))
        {
            //玩家胡牌操作
            if (IsAIPlayer == false)
            {
                //暂存胡牌数据
                WinOperation_Handler.GameOver = new GameOver
                {
                    overType = OverType.SelfTouch,
                    winPlayerId = PlayerId,
                    fromPlayerId = PlayerId,
                    mahJongType = mahJongType,
                };
                //设置赢牌操作数据
                winOperationData.fromPlayerId = PlayerId;
                winOperationData.toPlayerId = PlayerId;
                //玩家胡牌
                GameNetMessage.WinOperation_Handler.SendMessage(RoomId, PlayerId, winOperationData);
            }
            else
            {
                //AI玩家胡牌
            }
            return;
        }

        //玩家出牌
        PlayingTile();

        if(IsAIPlayer == true)//如果是AI玩家
        {
            //异步执行
            Task.Run(async () =>
            {
                //等待1秒
                await Task.Delay(1000);
                //AI玩家打出牌
                //随机生成手牌中的一个麻将
                mahJongType = RoomMahJongTilesManager.roomTiles[RoomId].mPlayerTiles[PlayerId].tiles[
                    System.Random.Shared.Next(0, RoomMahJongTilesManager.roomTiles[RoomId].mPlayerTiles[PlayerId].tiles.Count)];
                //更改牌堆数据
                RoomMahJongTilesManager.roomTiles[RoomId].PlayTile(PlayerId, mahJongType);
                //发送消息：玩家打出牌
                GameNetMessage.PlayerPlayedATile_Handler.SendMessage(RoomId, PlayerId, mahJongType);
                //玩家打出了一张牌
                RoomGameManager.RoomGames[RoomId].PlayerPlayingATile(PlayerId, mahJongType);


            });

        }

    }

    #region 设置玩家状态

    /// <summary>
    /// 出牌
    /// </summary>
    public virtual void PlayingTile()
    {
        //设置玩家状态
        State = PlayerState.Playing;
    }

    /// <summary>
    /// 玩家出了牌
    /// </summary>
    /// <param name="mahJongType"></param>
    public virtual void PlayedTile(MahJongType mahJongType)
    {
        //设置玩家状态
        State = PlayerState.Idle;
    }

    /// <summary>
    /// 玩家胡了
    /// </summary>
    public virtual void Win(MahJongType mahJongType)
    {
        //设置状态
        State = PlayerState.GameOver;
    }

    /// <summary>
    /// 玩家准备重新开始游戏
    /// </summary>
    public virtual void PrepareRestart()
    {
        //设置状态
        State = PlayerState.PreparingRestart;
    }

    #endregion

    /// <summary>
    /// 玩家检查出牌
    /// </summary>
    public virtual void EatTile(int roomId, EatTileOperation eatTileOperation)
    {
        Console.WriteLine($"有可以吃的牌了，玩家：{PlayerId}");

        //设置玩家状态
        State = PlayerState.Eat;
        if (IsAIPlayer == false)//是真人玩家
        {
            //发送消息：有可以吃的牌了
            GameNetMessage.PlayerHaveTileToEat_Handler.SendMessage(roomId, eatTileOperation.toPlayerId, eatTileOperation);
        }
        else//是AI玩家
        {
            //回调
            RoomGameManager.RoomGames[roomId].TotalNumberOfOperationsCurrentlyInProgress.Add(eatTileOperation);
        }
    }



    /// <summary>
    /// 玩家听牌
    /// </summary>
    public virtual void Listening()
    {
        //听牌
        IsListening = true;

        //调试用[*不重要代码]
        string str = "{";
        ListeningTiles.listenItemList.ForEach(item1 => item1.winTiles.ForEach(item2 => { str += item2.ToString() + ","; }));
        str += "}";
        Console.WriteLine($"玩家{PlayerId}听牌，胡牌为：{str}");

        //向玩家发送消息：给出宝牌
        GameNetMessage.SendTreasure_Handler.SendMessage(RoomId, PlayerId, RoomMahJongTilesManager.roomTiles[RoomId].Treasure);
    }

    /// <summary>
    /// 检查玩家是否胡牌。TODO：这个函数可以移到其它的类中去
    /// </summary>
    /// <param name="mahJongType">打出或自摸的哪张牌</param>
    /// <param name="selfTouch">是否为自摸</param>
    public virtual bool CheckWin(ref WinOperationData winOperationData, bool selfTouch = false)
    {
        //没有胡牌数据，默认不胡
        if (ListeningTiles.eatMahJongTypeList == null) return false;

        //获取到要胡的牌
        MahJongType mahJongType = winOperationData.mahJongType;

        if (selfTouch == true)//如果是自摸
        {
            if (mahJongType == MahJongType.RedDragon//自摸到红中
            || mahJongType == RoomMahJongTilesManager.roomTiles[RoomId].Treasure)//或 自摸到宝牌
            {
                //自摸到红中
                winOperationData.winOperationType = mahJongType == MahJongType.RedDragon
                    ? WinOperationType.SelfTouch_RedDragon : WinOperationType.SelfTouch_Treasure;
                //胡牌需要的牌
                List<MahJongType> fitTiles = new List<MahJongType>();
                ListeningTiles.listenItemList.ForEach(p1 =>
                {
                    if (p1.listenType != ListenType.StrongWind)//不是刮大风，因为刮大风会有不是手牌的牌
                    {
                        if (p1.listenType == ListenType.TwoDoubleTile) p1.otherTiles.ForEach(p2 => fitTiles.Add(p2));
                        p1.listenTiles.ForEach(p2 => fitTiles.Add(p2));
                    }
                });
                winOperationData.fitTiles = fitTiles;
                //玩家胡了
                return true;
            }
            else if(winOperationData.winOperationType == WinOperationType.SelfTouch_StrongWind)//如果是刮大风
            {
                ListenItem listenItem = ListeningTiles.listenItemList.FirstOrDefault(p => p.listenType == ListenType.StrongWind && p.winTiles.Contains(mahJongType));
                if (listenItem.listenType != default) //刮大风了
                {
                    //自摸
                    winOperationData.winOperationType = WinOperationType.SelfTouch_StrongWind;

                    //胡牌需要的牌
                    winOperationData.fitTiles = listenItem.listenTiles;

                    Console.WriteLine($"玩家{PlayerId}胡了");
                    //玩家胡了
                    return true;
                }
            }
            else//自摸到胡牌
            {
                ListenItem listenItem = ListeningTiles.listenItemList.FirstOrDefault(p => p.listenType != ListenType.StrongWind && p.winTiles.Contains(mahJongType));
                if (listenItem.listenType != default) //有值 
                {
                    //自摸 或 不是（其他人打出的）
                    winOperationData.winOperationType = WinOperationType.SelfTouch_EatTile;

                    if (listenItem.listenType == ListenType.TwoDoubleTile)//胡 两个对
                    {
                        //胡牌需要的牌
                        winOperationData.fitTiles = listenItem.listenTiles.Contains(mahJongType) ?
                            listenItem.listenTiles : listenItem.otherTiles;
                    }
                    else
                    {
                        //胡牌需要的牌
                        winOperationData.fitTiles = listenItem.listenTiles;
                    }

                    Console.WriteLine($"玩家{PlayerId}胡了");
                    //玩家胡了
                    return true;
                }
            }
        }
        else//不是自摸
        {
            ListenItem listenItem = ListeningTiles.listenItemList.FirstOrDefault(p => p.listenType != ListenType.StrongWind && p.winTiles.Contains(mahJongType));
            if (listenItem.listenType != default && listenItem.listenType != ListenType.StrongWind) //有值 
            {
                //自摸 或 不是（其他人打出的）
                winOperationData.winOperationType = selfTouch == true ?
                    WinOperationType.SelfTouch_EatTile : WinOperationType.OtherPlayed;

                if (listenItem.listenType == ListenType.TwoDoubleTile)//胡 两个对
                {
                    //胡牌需要的牌
                    winOperationData.fitTiles = listenItem.listenTiles.Contains(mahJongType) ?
                        listenItem.listenTiles : listenItem.otherTiles;
                }
                else
                {
                    //胡牌需要的牌
                    winOperationData.fitTiles = listenItem.listenTiles;
                }

                Console.WriteLine($"玩家{PlayerId}胡了");
                //玩家胡了
                return true;
            }
        }


        return false;
    }



}

public enum PlayerState
{
    None,
    /// <summary>出牌阶段</summary>
    Playing,
    /// <summary>闲置阶段（不能操作）</summary>
    Idle,
    /// <summary>吃牌阶段，正在判断是否吃牌</summary>
    Eat,
    /// <summary>准备听牌阶段</summary>
    ChooseWhetherToListen,
    /// <summary>选择是否胡牌阶段</summary>
    ChooseWhetherToWin,
    /// <summary>游戏结束阶段</summary>
    GameOver,
    /// <summary>正在准备再来一局</summary>
    PreparingRestart,

}
