using _2DSurviveGameServer._03Svc;
using PENet;
using Protocol;

namespace _2DSurviveGameServer._01Common
{
    public class ServerSession : KCPSession<Msg>
    {
        protected override void OnConnected()
        {
            this.Log("Client Connected sid:" + this.m_sid);
        }

        protected override void OnDisConnected()
        {
            this.Log("Client DisConnected sid:"+this.m_sid);
        }

        protected override void OnReciveMsg(Msg msg)
        {
            NetSvc.Instance.AddMsgQueue(this, msg);
        }

        protected override void OnUpdate(DateTime now)
        {
        }
    }
}
