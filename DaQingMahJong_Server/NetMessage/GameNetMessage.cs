using DaQingMahJong_Server.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VgeGameServer.Logic;
using VgeGameServer.Network;
using static DaQingMahJong_Server.NetMessage.NetMessage;

namespace DaQingMahJong_Server.NetMessage
{
    public class GameNetMessage : NetMessageBase
    {

        /// <summary>
        /// 剩余牌数的消息
        /// 服务端->客户端：告诉玩家现在的剩余牌数
        /// </summary>
        public struct RemainingTileCount
        {
            /// <summary>剩余牌数</summary>
            public int count;
        }

        public class RemainingTileCount_Handler : NetMessageHandlerBase<RemainingTileCount>
        {
            /// <summary>
            /// 向客户端发送消息
            /// </summary>
            /// <param name="count"></param>
            public static void SendMessage(int count)
            {
                RemainingTileCount remainingTileCount = new RemainingTileCount();
                remainingTileCount.count = count;
                //向客户端发送消息
                SendMessage(remainingTileCount);
            }

        }

        /// <summary>
        /// 发放初始手牌的消息
        /// 服务端->客户端：通知玩家初始的手牌
        /// </summary>
        public struct InitTiles
        {
            /// <summary>key:玩家Id，value:初始手牌</summary>
            public Dictionary<int, List<MahJongType>> tiles;
        }

        public class InitTiles_Handler : NetMessageHandlerBase<InitTiles>
        {
            /// <summary>
            /// 向指定房间的客户端发送消息
            /// </summary>
            /// <param name="count"></param>
            public static void SendMessage(int roomId, Dictionary<int, List<MahJongType>> tiles)
            {
                InitTiles initTiles = new InitTiles();
                initTiles.tiles = tiles;
                //向客户端发送消息
                SendMessage(roomId, initTiles);
            }

        }

        /// <summary>
        /// 发了一张牌的消息
        /// 服务端->客户端：通知玩家，服务端给某个玩家发了一张牌
        /// </summary>
        public struct DealATileToPlayer
        {
            /// <summary>玩家Id</summary>
            public int playerId;
            /// <summary>发的牌</summary>
            public MahJongType mahJongType;
        }

        public class DealATileToPlayer_Handler : NetMessageHandlerBase<DealATileToPlayer>
        {
            public static void SendMessage(int roomId, int playerId, MahJongType mahJongType)
            {
                //构造消息
                DealATileToPlayer dealATileToPlayer = new DealATileToPlayer();
                dealATileToPlayer.playerId = playerId;
                dealATileToPlayer.mahJongType = mahJongType;
                //向房间中广播消息
                SendMessage(roomId, dealATileToPlayer);
            }
        }

        /// <summary>
        /// 有玩家打出了一张牌
        /// 服务端->客户端：通知房间内的所有玩家，有人打出了一张牌
        /// 客户端->服务端：玩家出牌操作
        /// </summary>
        public struct PlayerPlayedATile
        {
            /// <summary>玩家Id</summary>
            public int playerId;
            /// <summary>打出的牌</summary>
            public MahJongType mahJongType;
        }


        public class PlayerPlayedATile_Handler : NetMessageHandlerBase<PlayerPlayedATile>
        {
            public static void SendMessage(int roomId, int playerId, MahJongType mahJongType)
            {
                //构造消息
                PlayerPlayedATile playerPlayedATile = new PlayerPlayedATile();
                playerPlayedATile.playerId = playerId;
                playerPlayedATile.mahJongType = mahJongType;
                //向房间中广播消息
                SendMessage(roomId, playerPlayedATile);
            }

            public override void OnMessage(ClientState clientState, PlayerPlayedATile netMessage)
            {
                //获取玩家
                Player player = PlayerManager.GetPlayer(clientState.clientID);
                //获取房间号
                int roomId = RoomManager.GetRoom(player.roomID).roomID;
                //判断玩家是否能出牌
                if(RoomGameManager.RoomGames[roomId].Players[player.playerIndex].State != PlayerState.Playing)//如果玩家不在出牌阶段
                {
                    //不能打出牌
                    return;
                }
                //设置玩家状态
                RoomGameManager.RoomGames[roomId].Players[player.playerIndex].PlayedTile(netMessage.mahJongType);
                //更改牌堆数据
                RoomMahJongTilesManager.roomTiles[roomId].PlayTile(player.playerIndex, netMessage.mahJongType);
                //向当前房间的其他玩家广播消息
                SendMessage(roomId, netMessage, clientState);
                //判断玩家是否听牌
                if (RoomGameManager.RoomGames[roomId].Players[player.playerIndex].IsListening == false //如果玩家没有听牌
                    && RoomMahJongTilesManager.roomTiles[roomId].CheckListening(player.playerIndex) == true //玩家可以听牌
                    )
                {
                    //向指定玩家发送消息：是否要听牌？
                    PlayerListenOperation_Handler.SendMessage(roomId, player.playerIndex, netMessage.mahJongType);
                }
                else
                {
                    //有玩家打出了一张牌
                    RoomGameManager.RoomGames[roomId].PlayerPlayingATile(player.playerIndex, netMessage.mahJongType);
                }
            }
        }


        /// <summary>
        /// 玩家有牌可以吃了
        /// 服务端->客户端：通知玩家可以进行吃牌操作
        /// 客户端->服务端：玩家进行了吃牌操作
        /// </summary>
        public struct PlayerHaveTileToEat
        {
            /// <summary>吃牌操作，包含回调</summary>
            public EatTileOperation eatTileOperation;
        }


        public class PlayerHaveTileToEat_Handler : NetMessageHandlerBase<PlayerHaveTileToEat>
        {
            public static void SendMessage(int roomId, int playerId, EatTileOperation eatTileOperation)
            {
                //获取玩家
                ClientState clientState = RoomManager.GetRoom(roomId).GetPlayer(playerId).clientState;
                //构造消息
                PlayerHaveTileToEat playerHaveTileToEat = new PlayerHaveTileToEat();
                playerHaveTileToEat.eatTileOperation = eatTileOperation;
                //给指定玩家发送消息
                SendMessage(clientState, playerHaveTileToEat);

            }

            public override void OnMessage(ClientState clientState, PlayerHaveTileToEat netMessageBase)
            {
                //获取房间Id
                int roomId = PlayerManager.GetPlayer(clientState.clientID).roomID;
                //玩家操作完成
                RoomGameManager.RoomGames[roomId].TotalNumberOfOperationsCurrentlyInProgress.Add(netMessageBase.eatTileOperation);
            }

        }

        /// <summary>
        /// 玩家吃了一张牌
        /// 服务端->客户端：通知一个房间内的玩家，有玩家吃牌了
        /// </summary>
        public struct PlayerAteATile
        {
            /// <summary>玩家Id</summary>
            public int playerId;
            /// <summary>吃牌操作</summary>
            public EatTileOperation eatTileOperation;
        }

        public class PlayerAteATile_Handler : NetMessageHandlerBase<PlayerAteATile>
        {
            public static void SendMessage(int roomId, int playerId, EatTileOperation eatTileOperation)
            {
                //构造消息
                PlayerAteATile playerAteATile = new PlayerAteATile();
                playerAteATile.playerId = playerId;
                playerAteATile.eatTileOperation = eatTileOperation;
                //向房间内的其他玩家发送消息
                SendMessage(roomId, playerAteATile);
            }
        }

        /// <summary>
        /// 玩家的听牌操作
        /// 服务端->客户端：通知玩家可以进行听牌操作
        /// 客户端->服务端：玩家传回听牌操作
        /// </summary>
        public struct PlayerListenOperation
        {
            /// <summary>是否要听牌</summary>
            public bool whetherToListen;
            /// <summary>打出的哪张牌（暂存）</summary>
            public MahJongType mahJongType;
        }

        public class PlayerListenOperation_Handler : NetMessageHandlerBase<PlayerListenOperation>
        {
            public static void SendMessage(int roomId, int playerId, MahJongType mahJongType)
            {
                //获取玩家
                ClientState clientState = RoomManager.GetRoom(roomId).GetPlayer(playerId).clientState;
                //构造消息
                PlayerListenOperation playerListenOperation = new PlayerListenOperation();
                playerListenOperation.mahJongType = mahJongType;
                //给指定玩家发送消息
                SendMessage(clientState, playerListenOperation);
            }

            public override void OnMessage(ClientState clientState, PlayerListenOperation netMessage)
            {
                Player player = PlayerManager.GetPlayer(clientState.clientID);
                if (netMessage.whetherToListen == true)//如果玩家选择听牌
                {
                    RoomGameManager.RoomGames[player.roomID].Players[player.playerIndex].Listening();
                }
                //玩家打出了一张牌
                RoomGameManager.RoomGames[player.roomID].PlayerPlayingATile(player.playerIndex, netMessage.mahJongType);
            }
        }

        /// <summary>
        /// 给玩家宝牌
        /// 服务端->客户端：给玩家发宝牌
        /// </summary>
        public struct SendTreasure
        {
            /// <summary>玩家Id</summary>
            public int playerId;
            /// <summary>宝牌</summary>
            public MahJongType mahJongType;
        }

        public class SendTreasure_Handler : NetMessageHandlerBase<SendTreasure>
        {
            public static void SendMessage(int roomId, int playerId, MahJongType mahJongType)
            {
                //构造消息
                SendTreasure sendTreasure = new SendTreasure();
                sendTreasure.playerId = playerId;
                sendTreasure.mahJongType = mahJongType;
                //向房间内的玩家发送消息：该玩家已听牌
                SendMessage(roomId, sendTreasure);
            }
        }

        /// <summary>
        /// 玩家胡牌操作
        /// 服务端->客户端：玩家可以胜利了
        /// 客户端->服务端：是否胡牌
        /// </summary>
        public struct WinOperation
        {
            /// <summary>胡牌操作需要的数据</summary>
            public WinOperationData winOperationData;
            /// <summary>玩家是否要赢，仅客户端传回服务端有用</summary>
            public bool whetherToWin;
        }

        public class WinOperation_Handler : NetMessageHandlerBase<WinOperation>
        {
            /// <summary>暂存游戏结束数据</summary>
            public static GameOver GameOver { get; set; }

            public static void SendMessage(int roomId, int playerId, WinOperationData winOperationData)
            {
                //获取玩家
                ClientState clientState = RoomManager.GetRoom(roomId).GetPlayer(playerId).clientState;
                //构造消息
                WinOperation winOperation = new WinOperation();
                winOperation.winOperationData = winOperationData;
                //给指定玩家发送消息：你要赢吗？
                SendMessage(clientState, winOperation);
            }


            public override void OnMessage(ClientState clientState, WinOperation netMessage)
            {
                Player player = PlayerManager.GetPlayer(clientState.clientID);
                if (netMessage.whetherToWin == true)
                {
                    //玩家胜利
                    RoomGameManager.RoomGames[player.roomID].Players[player.playerIndex].Win(netMessage.winOperationData.mahJongType);
                    //发送消息：游戏结束
                    GameNetMessage.GameOver_Handler.SendMessage(GameOver, player.roomID);
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// 游戏结束消息
        /// 服务端->客户端：通知房间内的玩家游戏结束
        /// </summary>
        public struct GameOver
        {
            /// <summary>结束的方式</summary>
            public OverType overType;
            /// <summary>赢的玩家</summary>
            public int winPlayerId;
            /// <summary>打出牌的玩家</summary>
            public int fromPlayerId;
            /// <summary>胡的哪张牌</summary>
            public MahJongType mahJongType;
        }

        public enum OverType
        {
            None, NoTileLeft, 
            /// <summary>自摸</summary>
            SelfTouch, 
            /// <summary>别人打出的牌</summary>
            OtherPlayed, 
        }

        public class GameOver_Handler : NetMessageHandlerBase<GameOver>
        {
            
            public static void SendMessage(GameOver gameOver, int roomId)
            {
                //发送消息
                SendMessage(roomId, gameOver);
            }
        }


        /// <summary>
        /// 重新开始游戏 消息
        /// 客户端->服务端：玩家想要再开一局
        /// 服务端->客户端：通知房间内的玩家，重新开始游戏
        /// </summary>
        public struct RestartGame
        {

        }

        public class RestartGame_Handler : NetMessageHandlerBase<RestartGame>
        {
            public static void SendMessage(int roomId)
            {
                //构造消息
                RestartGame restartGame = new RestartGame();
                //发送消息
                SendMessage(roomId, restartGame);
            }

            public override void OnMessage(ClientState clientState, RestartGame netMessageBase)
            {
                //获取玩家
                Player player = PlayerManager.GetPlayer(clientState.clientID);
                //获取房间Id
                int roomId = player.roomID;

                //玩家准备重新开始游戏
                RoomGameManager.RoomGames[roomId].Players[player.playerIndex].PrepareRestart();
                //判断是否有玩家没有准备
                GamePlayer? gamePlayer = RoomGameManager.RoomGames[roomId].Players.Values.FirstOrDefault(
                    p => !p.IsAIPlayer && p.State != PlayerState.PreparingRestart
                    );
                if(gamePlayer == null)//所有玩家都准备了
                {
                    //发送消息：重新开始游戏
                    SendMessage(roomId);
                    //重新开始游戏
                    RoomGameManager.RestartGame(roomId);
                }

            }
        }



    }
}
