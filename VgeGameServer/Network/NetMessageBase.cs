

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using VgeGameServer.Logic;

namespace VgeGameServer.Network;


public class NetMessageBase
{

    /// <summary>玩家离开游戏事件</summary>
    public static event Action<ClientState>? PlayerLeaveEvent;
    /// <summary>
    /// 呼叫事件：玩家离开游戏事件
    /// </summary>
    /// <param name="state"></param>
    internal static void CallPlayerLeaveEvent(ClientState state)
    {
        PlayerLeaveEvent?.Invoke(state);
    }


    /// <summary>
    /// 心跳消息，用于检测客户端是否还在连接中，客户端主动发送->服务端进行响应
    /// </summary>
    public class PingPong
    {

    }

    /// <summary>
    /// 心跳消息的处理类
    /// </summary>
    public class PingPong_Handler : NetMessageHandlerBase<PingPong>
    {
        public static void SendMessage(ClientState clientState)
        {
            //传回Pong消息
            PingPong netMessage = new PingPong();
            SendMessage(clientState, netMessage);

        }

        public override void OnMessage(ClientState clientState, PingPong netMessageBase)
        {
            //记录上一次Ping的时间
            clientState.lastPingTime = Network.GetTimeStamp();
            //传回Pong消息
            PingPong_Handler.SendMessage(clientState);
        }

    }













    ///// <summary>
    ///// 开始游戏消息
    ///// 服务端->客户端：向当前房间的另一名玩家发送Battle消息
    ///// 客户端->服务端：接收到另一名玩家发送的Battle消息
    ///// </summary>
    //public class ChatBattleText
    //{
    //    /// <summary>玩家昵称</summary>
    //    public string playerName;
    //    /// <summary>发送的消息</summary>
    //    public string text;
    //}

    ///// <summary>
    ///// 开始游戏消息的处理类
    ///// </summary>
    //public class ChatBattleText_Handler : NetMessageHandlerBase<ChatBattleText>
    //{
    //    public static void SendMessage(int roomId,string playerName, string text, ClientState clientState)
    //    {
    //        ChatBattleText chatBattleText = new ChatBattleText();
    //        chatBattleText.playerName = playerName;
    //        chatBattleText.text = text;
    //        //向当前房间的另一名玩家发送Battle消息
    //        SendMessage(roomId, chatBattleText, clientState);
    //    }


    //    public override void OnMessage(ClientState clientState, ChatBattleText battleText)
    //    {
    //        //获取玩家所在的房间
    //        int roomID = PlayerManager.GetPlayer(clientState.clientID).roomID;
    //        //向其他玩家同步状态信息
    //        SendMessage(roomID, battleText.playerName, battleText.text, clientState);

    //    }

    //}





}
