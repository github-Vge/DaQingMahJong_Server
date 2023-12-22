using DaQingMahJong_Server.NetMessage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static DaQingMahJong_Server.NetMessage.GameNetMessage;
using static DaQingMahJong_Server.NetMessage.NetMessage;

namespace DaQingMahJong_Server.GameLogic;

/// <summary>
/// 管理玩家麻将牌数据的类
/// </summary>
public class RoomMahJongTiles
{
    /// <summary>当前的房间Id</summary>
    public int roomId;
    /// <summary>当前的麻将列表</summary>
    public List<MahJongType> mCurrentMahJongList = new List<MahJongType>();
    /// <summary>4个玩家的牌堆，key：玩家Id，value：玩家的牌。默认玩家1是当前玩家</summary>
    public Dictionary<int, MahJongTiles> mPlayerTiles = new Dictionary<int, MahJongTiles>();
    /// <summary>宝牌</summary>
    public MahJongType Treasure { get; private set; }

    public RoomMahJongTiles(int roomId)
    {
        //当前的麻将列表（复制一份）
        mCurrentMahJongList = RoomMahJongTilesManager.mInitMahJongList.ToList();
        //生成宝牌
        GenerateTreasure();
        //新建玩家牌堆
        for (int i = 1; i <= 4; i++)
        {
            mPlayerTiles[i] = new MahJongTiles();
        }
        //显示剩余牌数
        RemainingTileCount_Handler.SendMessage(mCurrentMahJongList.Count);
        //记录房间号
        this.roomId = roomId;

    }

    /// <summary>
    /// 生成宝牌
    /// </summary>
    private void GenerateTreasure()
    {
        //随机一张牌
        int randomIndex = System.Random.Shared.Next(0, mCurrentMahJongList.Count);
        //记录宝牌
        Treasure = mCurrentMahJongList[randomIndex];
        //从麻将列表中移除
        mCurrentMahJongList.Remove(Treasure);
    }

    /// <summary>
    /// 给指定玩家发牌
    /// </summary>
    /// <param name="playerId">玩家Id（1-4）</param>
    /// <param name="tileCount">发牌数量</param>
    public void DealTiles(int playerId, int tileCount)
    {
        for (int i = 0; i < tileCount; i++)
        {
            //随机一张牌
            int randomIndex = System.Random.Shared.Next(0, mCurrentMahJongList.Count);
            //添加到玩家的手牌中
            mPlayerTiles[playerId].tiles.Add(mCurrentMahJongList[randomIndex]);
            //从麻将列表中移除
            mCurrentMahJongList.RemoveAt(randomIndex);
        }
        //显示剩余牌数（可以先不用）
        //RemainingTileCount_Handler.SendMessage(mCurrentMahJongList.Count);
    }

    /// <summary>
    /// 为玩家发一张牌
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns>发了哪张牌</returns>
    public MahJongType DealATile(int playerId)
    {
        //没牌了，游戏结束
        if (mCurrentMahJongList.Count == 0)
        {
            //构造消息
            GameOver gameOver = new GameOver
            {
                overType = OverType.NoTileLeft,
                winPlayerId = 0,
                fromPlayerId = 0,
                mahJongType = MahJongType.None,
            };
            //发送消息：游戏结束
            GameNetMessage.GameOver_Handler.SendMessage(gameOver, roomId);

            return MahJongType.None;
        }
        //随机一张牌
        int randomIndex = System.Random.Shared.Next(0, mCurrentMahJongList.Count);
        //获取牌的类型
        MahJongType mahJongType = mCurrentMahJongList[randomIndex];
        //添加到玩家的手牌中
        mPlayerTiles[playerId].tiles.Add(mahJongType);
        //从麻将列表中移除
        mCurrentMahJongList.RemoveAt(randomIndex);
        //更改剩余牌数
        GameNetMessage.RemainingTileCount_Handler.SendMessage(mCurrentMahJongList.Count);
        //返回发的牌
        return mahJongType;
    }

    /// <summary>
    /// 出牌
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="mahJongType"></param>
    public void PlayTile(int playerId, MahJongType mahJongType)
    {
        //从手牌中移除牌
        mPlayerTiles[playerId].tiles.Remove(mahJongType);
        //添加到打出的牌
        mPlayerTiles[playerId].playedTiles.Add(mahJongType);
        //手牌排序
        mPlayerTiles[playerId].tiles.Sort();
    }

    /// <summary>
    /// 吃牌
    /// </summary>
    /// <param name="eatTileOperation">吃牌的操作，其中包含了吃牌操作需要的信息</param>
    public void EatTile(EatTileOperation eatTileOperation)
    {
        //从出牌玩家的打出牌池中移除牌
        mPlayerTiles[eatTileOperation.fromPlayerId].playedTiles.Remove(eatTileOperation.mahJongType);
        //配合吃牌的牌列表
        List<MahJongType> fitEatMahJongTypeList = eatTileOperation.fitEatMahJongTypeList.ToList();
        //从吃牌玩家的手牌中移除配合吃牌的牌
        fitEatMahJongTypeList.ForEach(a => mPlayerTiles[eatTileOperation.toPlayerId].tiles.Remove(a));
        //添加需要吃的牌
        fitEatMahJongTypeList.Insert(1, eatTileOperation.mahJongType);
        //将需要吃的牌移入到吃牌玩家的吃牌牌池中
        mPlayerTiles[eatTileOperation.toPlayerId].eatTiles.Add(fitEatMahJongTypeList);
        //TODO:显示吃牌操作
        //MahJongManager.Instance.EatTile(eatTileOperation);
    }

    /// <summary>
    /// 检查玩家是否可以听牌
    /// </summary>
    /// <param name="playerId">玩家Id</param>
    /// <param name="mahJongType">麻将类型</param>
    /// <returns>是否可以听牌</returns>
    public bool CheckListening(int playerId)
    {
        //检查是否可以听牌
        bool checkListening = ListenManager.CheckListening(mPlayerTiles[playerId].tiles, mPlayerTiles[playerId].eatTiles, out ListeningTilesData listeningTilesData);
        //调试用[*不重要代码]
        //EatTileManager.Console.WriteLineTree(playerId);
        //暂存玩家的胡牌数据
        RoomGameManager.RoomGames[roomId].Players[playerId].ListeningTiles = checkListening == true ? listeningTilesData : default;
        //返回
        return checkListening;
    }
}

public enum MahJongType
{
    None,
    /// <summary>红中</summary>
    RedDragon,
    /// <summary>饼（1-9）</summary>
    Circle1, Circle2, Circle3, Circle4, Circle5, Circle6, Circle7, Circle8, Circle9,
    /// <summary>条（1-9）</summary>
    Stick1, Stick2, Stick3, Stick4, Stick5, Stick6, Stick7, Stick8, Stick9,
    /// <summary>万（1-9）</summary>
    Thousand1, Thousand2, Thousand3, Thousand4, Thousand5, Thousand6, Thousand7, Thousand8, Thousand9,

}


public class MahJongTiles
{
    /// <summary>玩家Id</summary>
    public int playerId;
    /// <summary>玩家的手牌</summary>
    public List<MahJongType> tiles = new List<MahJongType>();
    /// <summary>玩家吃的牌</summary>
    public List<List<MahJongType>> eatTiles = new List<List<MahJongType>>();
    /// <summary>玩家打出的牌</summary>
    public List<MahJongType> playedTiles = new List<MahJongType>();
}