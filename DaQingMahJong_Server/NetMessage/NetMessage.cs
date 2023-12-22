using DaQingMahJong_Server.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VgeGameServer.Logic;
using VgeGameServer.Network;
using static DaQingMahJong_Server.NetMessage.GameNetMessage;

namespace DaQingMahJong_Server.NetMessage
{
    public class NetMessage : NetMessageBase
    {
        public NetMessage():base()
        {
            NetMessageBase.PlayerLeaveEvent += LeaveRoom_Handler.OnPlayerLeaveEvent;
        }

        /// <summary>
        /// 登录消息
        /// 客户端->服务端：玩家登入
        /// 服务端->客户端：登录成功
        /// </summary>
        public class Login 
        {

        }

        /// <summary>
        /// 登录消息的处理类
        /// </summary>
        public class Login_Handler : NetMessageHandlerBase<Login>
        {
            public static void SendMessage(ClientState clientState)
            {
                //传回消息
                Login netMessage = new Login();
                SendMessage(clientState, netMessage);
            }

            /// <summary>
            /// 收到登录的消息后，就传回消息
            /// </summary>
            /// <param name="clientState"></param>
            /// <param name="netMessageBase"></param>
            public override void OnMessage(ClientState clientState, Login netMessageBase)
            {
                //新增玩家
                Player player = new Player(clientState);
                //添加玩家到全局字典中
                PlayerManager.AddPlayer(player.id, player);
                //更新时间戳
                clientState.lastPingTime = Network.GetTimeStamp();
                //传回消息
                Login_Handler.SendMessage(clientState);
                //TODO:发送房间列表消息
                //SendRoomListMessage();
            }

        }

        /// <summary>
        /// 加入房间消息
        /// 客户端->服务端：加入一个房间，并指定一个房间ID
        /// 服务端->客户端：返回玩家索引
        /// </summary>
        public class JoinRoom
        {
            /// <summary>房间Id</summary>
            public int roomID;
            /// <summary>玩家Id，玩家在房间的索引</summary>
            public int playerIndex;
        }

        /// <summary>
        /// 加入房间消息的处理类
        /// </summary>
        public class JoinRoom_Handler : NetMessageHandlerBase<JoinRoom>
        {
            public static void SendMessage(ClientState clientState)
            {
                //传回消息
                JoinRoom netMessage = new JoinRoom();
                netMessage.roomID = PlayerManager.GetPlayer(clientState.clientID).roomID;
                netMessage.playerIndex = PlayerManager.GetPlayer(clientState.clientID).playerIndex;
                SendMessage(clientState, netMessage);
            }

            public override void OnMessage(ClientState clientState, JoinRoom message)
            {
                //获取可用房间
                Room? room = RoomManager.GetAvailableRoom();
                if (room == null)//如果房间不存在，则新建一个房间
                {
                    room = RoomManager.AddRoom();
                }
                //向房间中添加玩家
                room.AddPlayer(clientState.clientID);
                //传回消息
                SendMessage(clientState);

                //如果房间人数满了，则开始游戏
                if (room.playerList.Count() == 2)
                {
                    //开始游戏
                    StartGame_Handler.SendMessage(clientState);
                }

            }

        }


        /// <summary>
        /// 离开房间消息
        /// 客户端->服务端：离开当前的房间
        /// 服务端->客户端：通知当前房间的其他玩家，有玩家退出了
        /// </summary>
        public class LeaveRoom
        {

        }

        /// <summary>
        /// 离开房间消息的处理类
        /// </summary>
        public class LeaveRoom_Handler : NetMessageHandlerBase<LeaveRoom>
        {
            /// <summary>
            /// clientState玩家离开房间
            /// </summary>
            /// <param name="roomId"></param>
            /// <param name="clientState"></param>
            public static void SendMessage(int roomId, ClientState clientState)
            {

            }

            public override void OnMessage(ClientState clientState, LeaveRoom netMessageBase)
            {

            }

            public static void OnPlayerLeaveEvent(ClientState clientState)
            {
                //玩家离开当前房间

            }

        }


        /// <summary>
        /// 开始游戏消息
        /// 服务端->客户端：通知指定房间的所有玩家开始游戏
        /// </summary>
        public class StartGame
        {
            /// <summary>玩家Id</summary>
            public int playerId;
            /// <summary>是否为AI玩家</summary>
            public List<bool> isAI = new List<bool>();
            //public int roomID;
            /// <summary>是否是房主</summary>
            public bool isRoomOwner;
        }

        /// <summary>
        /// 开始游戏消息的处理类
        /// </summary>
        public class StartGame_Handler : NetMessageHandlerBase<StartGame>
        {
            /// <summary>
            /// clientState玩家所在的房间开始游戏
            /// </summary>
            /// <param name="clientState"></param>
            public static void SendMessage(ClientState clientState)
            {
                //获取房间Id
                int roomId = PlayerManager.GetPlayer(clientState.clientID).roomID;
                //新开一场游戏
                RoomGameManager.StartANewGame(roomId);
                //向当前玩家所在房间的所有玩家发送游戏开始消息
                foreach (Player player in RoomManager.GetRoom(roomId).playerList)
                {
                    StartGame netMessage = new StartGame();
                    netMessage.isRoomOwner = player.isRoomOwner;
                    netMessage.playerId = player.playerIndex;
                    SendMessage(player.clientState, netMessage);
                }
                //发牌
                RoomGameManager.InitTiles(roomId);

            }

            public override void OnMessage(ClientState clientState, StartGame netMessageBase)
            {
                base.OnMessage(clientState, netMessageBase);
            }

        }


    }
}
