using _2DSurviveGameServer._01Common;
using _2DSurviveGameServer._03Svc;
using Protocol;

namespace _2DSurviveGameServer._02Sys
{
    public class PingSys:SysRoot<PingSys>
    {
        public override void Init()
        {
            base.Init();
            netSvc.AddMsgHandle(Protocol.CMD.ReqPing, ReqPing);
        }


        void ReqPing(MsgPack pack)
        {
            cacheSvc.UpdateUserHeartbeat(pack.msg.reqPing.uid,pack.session);

            pack.session.SendMsg(new Msg
            {
                cmd = CMD.RspPing,
                rspPing = new Protocol.Body.RspPing
                {
                    pid = pack.msg.reqPing.pid,
                }
            });
        }
    }
}
