using _2DSurviveGameServer._01Common;
using _2DSurviveGameServer._03Svc;
using Protocol;
using Protocol.Body;

namespace _2DSurviveGameServer._02Sys
{
    public class ChatSys : SysRoot<ChatSys>
    {
        Dictionary<long, List<ChatMessage>> offlineMessages;
        private readonly object cacheLock = new object();

        public override void Init()
        {
            base.Init();
            offlineMessages = new Dictionary<long, List<ChatMessage>>();
            netSvc.AddMsgHandle(Protocol.CMD.ChatMessage, ChatMessage);
            netSvc.AddMsgHandle(CMD.ReqChatHistory, HandleReqChatHistory);
        }

        public void ChatMessage(MsgPack pack)
        {
            ChatMessage chatMessage = pack.msg.chatMessage;

            if (string.IsNullOrEmpty(chatMessage.Message) || chatMessage.SendUID <= 0 || chatMessage.ReceiverUID <= 0)
            {
                // 无效消息处理逻辑
                return;
            }

            chatMessage.Timestamp = DateTime.UtcNow; // 更新消息时间戳

            lock (cacheLock)
            {
                // 获取接收者的会话
                ServerSession receiverSession = CacheSvc.Instance.GetSession(chatMessage.ReceiverUID);

                // 获取或创建聊天记录
                RspChatMessage rspChatMessages = CacheSvc.Instance.GetChatList(chatMessage.SendUID, chatMessage.ReceiverUID);
                if (rspChatMessages == null)
                {
                    rspChatMessages = new RspChatMessage();
                    rspChatMessages.chatList = new ChatMessage[] { chatMessage };
                    CacheSvc.Instance.AddChatList(chatMessage.SendUID, chatMessage.ReceiverUID, rspChatMessages);
                }
                else
                {
                    List<ChatMessage> tempList = rspChatMessages.chatList.ToList();
                    tempList.Add(chatMessage);
                    rspChatMessages.chatList = tempList.ToArray();
                }
                //自己也收一下吧；
                pack.session.SendMsg(new Protocol.Msg
                {
                    cmd = Protocol.CMD.RspChatMessage,
                    rspChatMessage = rspChatMessages
                });
                // 发送消息给接收者
                if (receiverSession != null && receiverSession.IsConnected())
                {
                    receiverSession.SendMsg(new Protocol.Msg
                    {
                        cmd = Protocol.CMD.RspChatMessage,
                        rspChatMessage = rspChatMessages
                    });
                }
                else
                {
                    if (!offlineMessages.ContainsKey(chatMessage.ReceiverUID))
                    {
                        offlineMessages[chatMessage.ReceiverUID] = new List<ChatMessage>();
                    }
                    offlineMessages[chatMessage.ReceiverUID].Add(chatMessage);
                }
            }
        }
        private void HandleReqChatHistory(MsgPack pack)
        {
            long uid = pack.msg.reqChatHistory.uId;
            long friendUid = pack.msg.reqChatHistory.friendUId;
            var key = CreateChatKey(uid, friendUid);
            var rspChatMessages = CacheSvc.Instance.GetChatList(uid,friendUid);
            if(rspChatMessages==null)return;
            var session = CacheSvc.Instance.GetSession(uid);
            if (session != null && session.IsConnected())
            {
                session.SendMsg(new Protocol.Msg
                {
                    cmd = CMD.RspChatMessage,
                    rspChatMessage = rspChatMessages,
                });
            }
        }
        private Tuple<long, long> CreateChatKey(long uid, long friendUid)
        {
            return uid < friendUid ? Tuple.Create(uid, friendUid) : Tuple.Create(friendUid, uid);
        }
    }

}
