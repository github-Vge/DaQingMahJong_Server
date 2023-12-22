using DaQingMahJong_Server.GameLogic;
using DaQingMahJong_Server.NetMessage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using static DaQingMahJong_Server.NetMessage.NetMessage;

public static class EatTileManager
{

    /// <summary>
    /// 检查其他玩家是否要吃牌
    /// </summary>
    /// <param name="playerId">打出牌的玩家</param>
    /// <param name="mahJongType"></param>
    /// <returns>false代表没有玩家要吃牌，true代表有玩家要吃牌</returns>
    public static bool CheckEat(int roomId, int playerId, MahJongType mahJongType)
    {
        //清空树的节点
        TreeNode<List<MahJongType>>.allTreeNodeList.Clear();

        //无牌时不需要判断（无牌即玩家已经打出了牌，经过吃牌阶段时，其他玩家没有吃牌操作后，有None类型的牌，即无牌）
        if (mahJongType == MahJongType.None) return false;

        //需要检查的玩家列表
        List<int> checkPlayerIdList = new List<int>();
        //遍历4个玩家
        for (int i = 1; i <= 4; i++)
        {
            //如果是出牌玩家 或者 玩家已经听牌，则不需要吃牌判断
            if (i == playerId
                || RoomGameManager.RoomGames[roomId].Players[i].IsListening == true
                )
            {
                continue;
            }
            checkPlayerIdList.Add(i);
        }


        //记录吃牌优先级（1最高，最优先）
        int eatTilePriority = 1;
        //每个玩家可以吃的牌的字典，key:玩家索引，value:所有可以吃的牌以及吃牌优先级
        List<EatTileOperation> eatTileOperationList = new List<EatTileOperation>();
        //配合吃牌的牌
        List<MahJongType> fitEatMahJongTypeList = new List<MahJongType>();


        //吃牌并听牌 或 碰牌并听牌 判断
        foreach (int checkPlayerId in checkPlayerIdList)
        {
            List<MahJongType> listeningMagJongTypeList = new List<MahJongType>();
            //可以右吃牌并听牌
            if (ListenManager.CheckEatAndListening(EatTileType.RightEat, mahJongType
                , RoomMahJongTilesManager.roomTiles[roomId].mPlayerTiles[checkPlayerId]
                , out fitEatMahJongTypeList, out listeningMagJongTypeList))
            {
                //可以右吃牌
                eatTileOperationList.Add(
                    new EatTileOperation
                    {
                        fromPlayerId = playerId,
                        toPlayerId = checkPlayerId,
                        eatTileType = EatTileType.RightEatAndListening,
                        fitEatMahJongTypeList = fitEatMahJongTypeList,
                        mahJongType = mahJongType,
                        eatTilePriority = eatTilePriority,
                    });
                //优先级加1
                eatTilePriority++;
            }
            //可以中吃牌并听牌
            if (ListenManager.CheckEatAndListening(EatTileType.MiddleEat, mahJongType
                , RoomMahJongTilesManager.roomTiles[roomId].mPlayerTiles[checkPlayerId]
                , out fitEatMahJongTypeList, out listeningMagJongTypeList))
            {
                //可以中吃牌
                eatTileOperationList.Add(
                    new EatTileOperation
                    {
                        fromPlayerId = playerId,
                        toPlayerId = checkPlayerId,
                        eatTileType = EatTileType.MiddleEatAndListening,
                        fitEatMahJongTypeList = fitEatMahJongTypeList,
                        mahJongType = mahJongType,
                        eatTilePriority = eatTilePriority,
                    });
                //优先级加1
                eatTilePriority++;
            }

            //可以左吃牌并听牌
            if (ListenManager.CheckEatAndListening(EatTileType.LeftEat, mahJongType
                , RoomMahJongTilesManager.roomTiles[roomId].mPlayerTiles[checkPlayerId]
                , out fitEatMahJongTypeList, out listeningMagJongTypeList))
            {
                //可以左吃牌
                eatTileOperationList.Add(
                    new EatTileOperation
                    {
                        fromPlayerId = playerId,
                        toPlayerId = checkPlayerId,
                        eatTileType = EatTileType.LeftEatAndListening,
                        fitEatMahJongTypeList = fitEatMahJongTypeList,
                        mahJongType = mahJongType,
                        eatTilePriority = eatTilePriority,
                    });
                //优先级加1
                eatTilePriority++;
            }
            //可以碰牌并听牌
            if (ListenManager.CheckEatAndListening(EatTileType.Touch, mahJongType
                , RoomMahJongTilesManager.roomTiles[roomId].mPlayerTiles[checkPlayerId]
                , out fitEatMahJongTypeList, out listeningMagJongTypeList))
            {
                //可以碰牌
                eatTileOperationList.Add(
                    new EatTileOperation
                    {
                        fromPlayerId = playerId,
                        toPlayerId = checkPlayerId,
                        eatTileType = EatTileType.TouchAndListening,
                        fitEatMahJongTypeList = fitEatMahJongTypeList,
                        mahJongType = mahJongType,
                        eatTilePriority = eatTilePriority,
                    });
                //优先级加1
                eatTilePriority++;
            }

        }


        //杠牌判断
        foreach (int checkPlayerId in checkPlayerIdList)
        {
            if (CheckGang(mahJongType
                , RoomMahJongTilesManager.roomTiles[roomId].mPlayerTiles[checkPlayerId]
                , out fitEatMahJongTypeList))//可以碰牌
            {
                //可以碰牌
                eatTileOperationList.Add(
                    new EatTileOperation
                    {
                        fromPlayerId = playerId,
                        toPlayerId = checkPlayerId,
                        eatTileType = EatTileType.Gang,
                        fitEatMahJongTypeList = fitEatMahJongTypeList,
                        mahJongType = mahJongType,
                        eatTilePriority = eatTilePriority,
                    });
                //优先级加1
                eatTilePriority++;
            }

        }

        //碰牌判断
        foreach (int checkPlayerId in checkPlayerIdList)
        {

            if (CheckTouch(mahJongType
                , RoomMahJongTilesManager.roomTiles[roomId].mPlayerTiles[checkPlayerId]
                , out fitEatMahJongTypeList))//可以碰牌
            {
                //可以碰牌
                eatTileOperationList.Add(
                    new EatTileOperation
                    {
                        fromPlayerId = playerId,
                        toPlayerId = checkPlayerId,
                        eatTileType = EatTileType.Touch,
                        fitEatMahJongTypeList = fitEatMahJongTypeList,
                        mahJongType = mahJongType,
                        eatTilePriority = eatTilePriority,
                    });
                //优先级加1
                eatTilePriority++;
            }


        }

        //吃牌判断
        foreach (int checkPlayerId in checkPlayerIdList)
        {

            if (checkPlayerId == playerId % 4 + 1) //是下一个玩家
            {

                if (CheckRightEat(mahJongType
                    , RoomMahJongTilesManager.roomTiles[roomId].mPlayerTiles[checkPlayerId]
                    , out fitEatMahJongTypeList))//可以右吃牌
                {
                    //可以右吃牌
                    eatTileOperationList.Add(
                        new EatTileOperation
                        {
                            fromPlayerId = playerId,
                            toPlayerId = checkPlayerId,
                            eatTileType = EatTileType.RightEat,
                            fitEatMahJongTypeList = fitEatMahJongTypeList,
                            mahJongType = mahJongType,
                            eatTilePriority = eatTilePriority,
                        });
                    //优先级加1
                    eatTilePriority++;
                }
                if (CheckMiddleEat(mahJongType
                    , RoomMahJongTilesManager.roomTiles[roomId].mPlayerTiles[checkPlayerId]
                    , out fitEatMahJongTypeList))//可以中吃牌
                {
                    //可以中吃牌
                    eatTileOperationList.Add(
                        new EatTileOperation
                        {
                            fromPlayerId = playerId,
                            toPlayerId = checkPlayerId,
                            eatTileType = EatTileType.MiddleEat,
                            fitEatMahJongTypeList = fitEatMahJongTypeList,
                            mahJongType = mahJongType,
                            eatTilePriority = eatTilePriority,
                        });
                    //优先级加1
                    eatTilePriority++;
                }

                if (CheckLeftEat(mahJongType
                    , RoomMahJongTilesManager.roomTiles[roomId].mPlayerTiles[checkPlayerId]
                    , out fitEatMahJongTypeList))//可以左吃牌
                {
                    //可以左吃牌
                    eatTileOperationList.Add(
                        new EatTileOperation
                        {
                            fromPlayerId = playerId,
                            toPlayerId = checkPlayerId,
                            eatTileType = EatTileType.LeftEat,
                            fitEatMahJongTypeList = fitEatMahJongTypeList,
                            mahJongType = mahJongType,
                            eatTilePriority = eatTilePriority,
                        });
                    //优先级加1
                    eatTilePriority++;
                }
            }


        }

        if (eatTileOperationList.Count == 0)//没有需要吃牌的玩家
        {
            return false;
        }

        //清空吃牌操作回调池
        RoomGameManager.RoomGames[roomId].TotalNumberOfOperationsCurrentlyInProgress.Clear();

        //对每个需要吃的牌进行操作（是否要吃？）
        foreach (EatTileOperation eatTileOperation in eatTileOperationList)
        {
            //呼叫事件，有牌吃了[!重要代码]
            RoomGameManager.RoomGames[roomId].Players[eatTileOperation.toPlayerId].EatTile(roomId, eatTileOperation);
        }

        //开始监听吃牌操作
        Task.Run(async () =>
        {
            await EatTileRoundCallback(roomId, eatTilePriority - 1);
        });


        return true;

    }

    #region 检查对应吃牌类型是否有可以吃的牌

    /// <summary>
    /// 检查是否有左吃牌，例1：有四万、五万，吃三万，例2：有二条、三条，吃一条。
    /// </summary>
    /// <param name="checkPlayerId">要吃牌的玩家Id</param>
    /// <param name="mahJongType">吃的哪张牌</param>
    /// <returns></returns>
    public static bool CheckLeftEat(MahJongType mahJongType, MahJongTiles mahJongTiles
        , out List<MahJongType> fitEatMahJongTypeList)
    {
        //初始化配合吃牌的牌列表
        fitEatMahJongTypeList = new List<MahJongType>();
        if (int.TryParse(Enum.GetName(typeof(MahJongType), mahJongType).Last().ToString(), out int number))//是数字牌
        {
            if (number < 8)
            {
                MahJongType mahJongType1 = mahJongTiles.tiles.FirstOrDefault(p => p == mahJongType + 1);
                MahJongType mahJongType2 = mahJongTiles.tiles.FirstOrDefault(p => p == mahJongType + 2);
                if (mahJongType1 != default && mahJongType2 != default)
                {
                    //Console.WriteLine($"玩家{checkPlayerId}有左吃牌！吃牌为{Enum.GetName(typeof(MahJongType), mahJongType)}");
                    //传回配合吃牌的牌
                    fitEatMahJongTypeList.Add(mahJongType1);
                    fitEatMahJongTypeList.Add(mahJongType2);
                    //可以左吃牌
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 检查是否有中吃牌
    /// </summary>
    /// <param name="checkPlayerId"></param>
    /// <param name="mahJongType">吃的哪张牌</param>
    /// <returns></returns>
    public static bool CheckMiddleEat(MahJongType mahJongType, MahJongTiles mahJongTiles
        , out List<MahJongType> fitEatMahJongTypeList)
    {
        //初始化配合吃牌的牌列表
        fitEatMahJongTypeList = new List<MahJongType>();
        if (int.TryParse(Enum.GetName(typeof(MahJongType), mahJongType).Last().ToString(), out int number))//是数字牌
        {
            if (number > 1 && number < 9)
            {
                MahJongType mahJongType1 = mahJongTiles.tiles.FirstOrDefault(p => p == mahJongType - 1);
                MahJongType mahJongType2 = mahJongTiles.tiles.FirstOrDefault(p => p == mahJongType + 1);
                if (mahJongType1 != default && mahJongType2 != default)
                {
                    //Console.WriteLine($"玩家{checkPlayerId}有中吃牌！吃牌为{Enum.GetName(typeof(MahJongType), mahJongType)}");
                    //传回配合吃牌的牌
                    fitEatMahJongTypeList.Add(mahJongType1);
                    fitEatMahJongTypeList.Add(mahJongType2);
                    //可以中吃牌
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 检查是否有右吃牌
    /// </summary>
    /// <param name="checkPlayerId"></param>
    /// <param name="mahJongType"></param>
    /// <param name="fitEatMahJongTypeList">配合吃牌的牌列表</param>
    /// <returns></returns>
    public static bool CheckRightEat(MahJongType mahJongType, MahJongTiles mahJongTiles
        , out List<MahJongType> fitEatMahJongTypeList)
    {
        //初始化配合吃牌的牌列表
        fitEatMahJongTypeList = new List<MahJongType>();
        if (int.TryParse(Enum.GetName(typeof(MahJongType), mahJongType).Last().ToString(), out int number))//是数字牌
        {
            if (number > 2)
            {
                MahJongType mahJongType1 = mahJongTiles.tiles.FirstOrDefault(p => p == mahJongType - 1);
                MahJongType mahJongType2 = mahJongTiles.tiles.FirstOrDefault(p => p == mahJongType - 2);
                if (mahJongType1 != default && mahJongType2 != default)
                {
                    //Console.WriteLine($"玩家{checkPlayerId}有右吃牌！吃牌为{Enum.GetName(typeof(MahJongType), mahJongType)}");
                    //传回配合吃牌的牌
                    fitEatMahJongTypeList.Add(mahJongType1);
                    fitEatMahJongTypeList.Add(mahJongType2);
                    //可以右吃牌
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 检查是否有碰牌
    /// </summary>
    /// <param name="checkPlayerId"></param>
    /// <param name="mahJongType"></param>
    /// <returns></returns>
    public static bool CheckTouch(MahJongType mahJongType, MahJongTiles mahJongTiles
        , out List<MahJongType> fitEatMahJongTypeList)
    {
        //初始化配合吃牌的牌列表
        fitEatMahJongTypeList = new List<MahJongType>();
        if (mahJongTiles.tiles.Count(p => p == mahJongType) >= 2)
        {
            //Console.WriteLine($"玩家{checkPlayerId}有碰牌！碰牌为{Enum.GetName(typeof(MahJongType), mahJongType)}");
            //传回配合吃牌的牌
            fitEatMahJongTypeList.Add(mahJongType);
            fitEatMahJongTypeList.Add(mahJongType);
            //可以碰牌
            return true;
        }

        return false;
    }

    /// <summary>
    /// 检查是否有杠牌
    /// </summary>
    /// <param name="checkPlayerId"></param>
    /// <param name="mahJongType"></param>
    /// <returns></returns>
    private static bool CheckGang(MahJongType mahJongType, MahJongTiles mahJongTiles
        , out List<MahJongType> fitEatMahJongTypeList)
    {
        //初始化配合吃牌的牌列表
        fitEatMahJongTypeList = new List<MahJongType>();
        if (mahJongTiles.tiles.Count(p => p == mahJongType) == 3)
        {
            //Console.WriteLine($"玩家{checkPlayerId}有杠牌！杠牌为{Enum.GetName(typeof(MahJongType), mahJongType)}");
            //传回配合吃牌的牌
            fitEatMahJongTypeList.Add(mahJongType);
            fitEatMahJongTypeList.Add(mahJongType);
            fitEatMahJongTypeList.Add(mahJongType);
            //可以杠牌
            return true;
        }

        return false;
    }


    /// <summary>
    /// 判断是否有懒子
    /// </summary>
    /// <param name="eatTiles">吃的牌</param>
    /// <param name="node">叶子节点，Count一定为3</param>
    /// <returns></returns>
    private static bool HasWildTile(List<List<MahJongType>> eatTiles, TreeNode<List<MahJongType>> node, List<MahJongType> remainingMahJongList)
    {
        //获取全部的牌
        List<List<MahJongType>> wholeMahJongTypeList = eatTiles.ToList();
        TreeNode<List<MahJongType>> checkNode = node;
        while (checkNode.Parent != null)
        {
            wholeMahJongTypeList.Add(checkNode.Data);
            checkNode = checkNode.Parent;
        }
        //添加能胡牌的牌
        wholeMahJongTypeList.Add(remainingMahJongList);

        //判断是否有红中
        if (wholeMahJongTypeList.FirstOrDefault(p => p.Contains(MahJongType.RedDragon)) != default)
        {
            //有红中，可以当癞子
            return true;
        }
        //判断是否有刻子（三个相同的）
        foreach (List<MahJongType> mahJongTypeList in wholeMahJongTypeList)
        {
            if (mahJongTypeList.Count == 3 && mahJongTypeList.ToHashSet().Count == 1)//三张牌的节点中有刻子
            {
                return true;
            }
        }

        //判断是否有两个对
        if (wholeMahJongTypeList.Last().Count(p1 => wholeMahJongTypeList.Last().Count(p2 => p2 == p1) == 2) == 4)
        {
            //两个对（两张牌的数量为2）
            return true;
        }

        //没有癞子
        return false;
    }


    /// <summary>
    /// 调试用，输出树的所有数据
    /// </summary>
    /// <param name="layerCount"></param>
    public static void Print(int playerId)
    {
        string log = string.Empty;
        log += $"玩家{playerId}-----\n";
        bool needLog = false;
        //输出整个树（调试用）
        for (int i = 1; i < 5; i++)
        {
            foreach (TreeNode<List<MahJongType>> node in TreeNode<List<MahJongType>>.GetNodesAtLayer(i))
            {
                string tiles = string.Empty;
                //获取节点所在枝上的所有麻将
                TreeNode<List<MahJongType>> exceptNode = node;
                for (int j = 0; j < i; j++)
                {
                    tiles += $"层级{i - j}:{{";
                    exceptNode.Data.ForEach(a => tiles += a.ToString() + ",");
                    tiles += "},";
                    exceptNode = exceptNode.Parent;
                }
                needLog = true;
                log += tiles + "\n";
            }
        }
        if (needLog)
            Console.WriteLine(log);
    }



    #endregion

    /// <summary>
    /// 吃牌回合的回调函数，一直运行直到所有玩家结束吃牌回合
    /// </summary>
    /// <param name="totalOperationCount">吃牌操作的计数</param>
    /// <returns></returns>
    private static async Task EatTileRoundCallback(int roomId, int totalOperationCount)
    {
        Console.WriteLine("吃牌等待中...");

        //监听所有操作
        List<EatTileOperation> progresses = RoomGameManager.RoomGames[roomId].TotalNumberOfOperationsCurrentlyInProgress;

        while (totalOperationCount != progresses.Count)
        {
            //等待操作完成
            await Task.Delay(100);
        }

        List<EatTileOperation> sortedOperations = progresses.OrderBy(p => p.eatTilePriority).ToList();

        foreach (EatTileOperation op in sortedOperations)
        {
            if (op.whetherToEat == true)
            {
                //如果是杠牌，则再发一张牌
                if (op.eatTileType == EatTileType.Gang || op.eatTileType == EatTileType.GangAndListening)
                {
                    //给玩家发一张牌
                    MahJongType mahJongType = RoomMahJongTilesManager.roomTiles[roomId].DealATile(op.toPlayerId);
                    //发送消息
                    GameNetMessage.DealATileToPlayer_Handler.SendMessage(roomId, op.toPlayerId, mahJongType);
                }

                //对应玩家吃牌操作（更新数据）
                RoomMahJongTilesManager.roomTiles[roomId].EatTile(op);

                //发送消息：玩家吃了牌
                GameNetMessage.PlayerAteATile_Handler.SendMessage(roomId, op.toPlayerId, op);

                //设置玩家状态
                RoomGameManager.RoomGames[roomId].Players[op.toPlayerId].PlayingTile();

                Console.WriteLine($"玩家{op.toPlayerId}吃了玩家{op.fromPlayerId}的牌，吃牌方式：{op.eatTileType}，牌：{op.mahJongType}");

                return;
            }
        }

        //所有玩家都不吃牌
        RoomGameManager.RoomGames[roomId].PlayerPlayedATile(sortedOperations[0].fromPlayerId, sortedOperations[0].mahJongType);

        return;
    }



}

public struct EatTileOperation
{
    /// <summary>出牌的玩家</summary>
    public int fromPlayerId;
    /// <summary>吃牌的玩家</summary>
    public int toPlayerId;
    /// <summary>哪张麻将牌</summary>
    public MahJongType mahJongType;
    /// <summary>配合吃牌的牌，例：吃二万，那配合吃牌的牌就是一万和三万两张牌。</summary>
    public List<MahJongType> fitEatMahJongTypeList;
    /// <summary>吃牌的方式</summary>
    public EatTileType eatTileType;
    /// <summary>吃牌优先级</summary>
    public int eatTilePriority;

    /// <summary>是否吃牌？仅由玩家传回此类时有用</summary>
    public bool whetherToEat;
}

/// <summary>
/// 玩家吃牌的状态
/// </summary>
public enum PlayerEatTileState
{

}


public enum EatTileType
{
    None,
    /// <summary>左吃牌，例：自己有8万、9万，别人打出了7万</summary>
    LeftEat,
    /// <summary>中吃牌，例：自己有4万、6万，别人打出了5万</summary>
    MiddleEat,
    /// <summary>右吃牌，例：自己有1万、2万，别人打出了3万</summary>
    RightEat,
    /// <summary>碰牌</summary>
    Touch,
    /// <summary>杠牌</summary>
    Gang,

    /// <summary>左吃牌，并可以听牌</summary>
    LeftEatAndListening,
    /// <summary>中吃牌，并可以听牌</summary>
    MiddleEatAndListening,
    /// <summary>右吃牌，并可以听牌</summary>
    RightEatAndListening,
    /// <summary>碰牌，并可以听牌</summary>
    TouchAndListening,
    /// <summary>杠牌，并可以听牌</summary>
    GangAndListening,


}

/// <summary>
/// 用于玩家胡牌判断的数据类
/// </summary>
public class WinTileCheckData
{
    /// <summary>胡牌项的列表</summary>
    public List<WinTileItem> winTileItemList = new List<WinTileItem>();

    public struct WinTileItem
    {
        /// <summary>胡的哪张牌</summary>
        public MahJongType mahJongType;
        /// <summary>胡牌需要的牌</summary>
        public List<MahJongType> fitTiles;
        /// <summary>胡牌的倍数（番数）</summary>
        public int factor;
    }
}



/// <summary>
/// 听牌的麻将的树结构
/// </summary>
/// <typeparam name="T"></typeparam>
public class TreeNode<T>
{
    public T Data { get; set; }

    /// <summary>当前节点的父节点</summary>
    public TreeNode<T> Parent { get; set; }
    //public List<TreeNode<T>> Children { get; set; }

    public TreeNode(T data)
    {
        this.Data = data;
        allTreeNodeList.Add(this);
    }

    /// <summary>
    /// 获取当前节点的层数
    /// </summary>
    /// <returns></returns>
    public int GetNodeLayer()
    {
        int layer = 0;

        TreeNode<T> temp = this;

        while (temp.Parent != null)
        {
            layer++;
            temp = temp.Parent;
        }

        return layer;
    }


    /// <summary>保存所有节点的列表</summary>
    public static List<TreeNode<T>> allTreeNodeList = new List<TreeNode<T>>();

    /// <summary>
    /// 获取树的某一层的所有节点
    /// </summary>
    /// <param name="layer"></param>
    /// <returns></returns>
    public static List<TreeNode<T>> GetNodesAtLayer(int layer)
    {
        List<TreeNode<T>> treeNodes = new List<TreeNode<T>>();

        foreach (TreeNode<T> node in allTreeNodeList)
        {
            if (node.GetNodeLayer() == layer)
            {
                treeNodes.Add(node);
            }
        }

        return treeNodes;
    }





}