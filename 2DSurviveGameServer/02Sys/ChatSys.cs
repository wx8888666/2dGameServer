using _2DSurviveGameServer._01Common;
using _2DSurviveGameServer._03Svc;
using Protocol;
using Protocol.Body;
using StackExchange.Redis;

namespace _2DSurviveGameServer._02Sys
{
    public class ChatSys : SysRoot<ChatSys>
    {
       // Dictionary<long, List<ChatMessage>> offlineMessages;
        private readonly object cacheLock = new object();
        private readonly IDatabase redisDb;
        public ChatSys()
        {
            redisDb = RedisManager.Connection.GetDatabase();
        }
        public override void Init()
        {
            base.Init();
            //offlineMessages = new Dictionary<long, List<ChatMessage>>();
            netSvc.AddMsgHandle(Protocol.CMD.ChatMessage, ChatMessage);
            netSvc.AddMsgHandle(CMD.ReqChatHistory, ReqChatHistory);
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
                // 生成聊天记录的Redis键
                string chatKey = CreateChatKey(chatMessage.SendUID, chatMessage.ReceiverUID);

                // 将消息存储到Redis中
                string serializedMessage = SerializeChatMessage(chatMessage);
                redisDb.ListRightPush(chatKey, serializedMessage);
                redisDb.KeyExpire(chatKey, TimeSpan.FromDays(7)); // 设置键的过期时间为7天

                // 自己也收一下消息
                pack.session.SendMsg(new Protocol.Msg
                {
                    cmd = Protocol.CMD.RspChatMessage,
                    rspChatMessage = new RspChatMessage
                    {
                        chatList = new[] { chatMessage }
                    }
                });

                // 发送消息给接收者
                if (receiverSession != null && receiverSession.IsConnected())
                {
                    receiverSession.SendMsg(new Protocol.Msg
                    {
                        cmd = Protocol.CMD.RspChatMessage,
                        rspChatMessage = new RspChatMessage
                        {
                            chatList = new[] { chatMessage }
                        }
                    });
                }
                else
                {
                    // 离线消息处理逻辑（可选）
                    //SaveOfflineMessage(chatMessage.ReceiverUID, serializedMessage);
                }
            }
        }
        private void ReqChatHistory(MsgPack pack)
        {
            long uid = pack.msg.reqChatHistory.uId;
            long friendUid = pack.msg.reqChatHistory.friendUId;
            string chatKey = CreateChatKey(uid, friendUid);

            var chatMessages = redisDb.ListRange(chatKey)
                                .Select(value => DeserializeChatMessage(value))
                                .ToArray();

            var session = CacheSvc.Instance.GetSession(uid);
            if (session != null && session.IsConnected())
            {
                session.SendMsg(new Protocol.Msg
                {
                    cmd = Protocol.CMD.RspChatMessage,
                    rspChatMessage = new RspChatMessage
                    {
                        chatList = chatMessages
                    }
                });
            }
        }
        private string CreateChatKey(long uid, long friendUid)
        {
            return uid < friendUid ? $"chat:{uid}:{friendUid}" : $"chat:{friendUid}:{uid}";
        }
        private string SerializeChatMessage(ChatMessage chatMessage)
        {
            // 将 ChatMessage 序列化为字符串
            return $"{chatMessage.SendUID},{chatMessage.ReceiverUID},{chatMessage.Timestamp},{chatMessage.Message}";
        }
        private ChatMessage DeserializeChatMessage(string serializedMessage)
        {
            // 将字符串反序列化为 ChatMessage
            var parts = serializedMessage.Split(',');
            return new ChatMessage
            {
                SendUID = long.Parse(parts[0]),
                ReceiverUID = long.Parse(parts[1]),
                Timestamp = DateTime.Parse(parts[2]),
                Message = parts[3]
            };
        }
    }

}
