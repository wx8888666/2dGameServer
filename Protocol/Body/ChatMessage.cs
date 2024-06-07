using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Body
{

    [Serializable]
    public class ChatMessage
    {
        public long SendUID { get; set; }
        public long ReceiverUID { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
    [Serializable]
    public class RspChatMessage
    {
       public ChatMessage[] chatList { get; set; }
    }
}
