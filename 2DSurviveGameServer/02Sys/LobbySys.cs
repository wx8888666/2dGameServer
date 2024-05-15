using _2DSurviveGameServer._01Common;
using _2DSurviveGameServer._02Sys.Room;
using _2DSurviveGameServer._03Svc;

namespace _2DSurviveGameServer._02Sys
{
    public class LobbySys:SysRoot<LobbySys>
    {
        Queue<long> matchQueue;

        public override void Init()
        {
            base.Init();
            matchQueue = new Queue<long>();

            netSvc.AddMsgHandle(Protocol.CMD.ReqJoinMatch, ReqJoinMatch);
            netSvc.AddMsgHandle(Protocol.CMD.ReqLeaveMatch, ReqLeaveMatch);
        }

        public override void Update()
        {
            base.Update();

            while(matchQueue.Count >= 2)
            {
                long[] uidArr = new long[2];
                for(int i = 0;i<2;i++)
                {
                    uidArr[i] = matchQueue.Dequeue();
                }
                RoomSys.Instance.Add(uidArr);
                this.Log($"匹配成功，当前匹配队列人数：{matchQueue.Count}");
            }
        }

        void ReqJoinMatch(MsgPack pack)
        {
            if (!matchQueue.Contains(pack.msg.reqJoinMatch.uid))
            {
                matchQueue.Enqueue(pack.msg.reqJoinMatch.uid);
            }

            pack.session.SendMsg(new Protocol.Msg
            {
                cmd = Protocol.CMD.RspJoinMatch,
            });

            //更新当前uid对应的客户端的状态
            cacheSvc.UpdateClientState(pack.msg.reqJoinMatch.uid,Protocol.Body.ClientStateEnum.MatchingQueue);

            this.Log("新增一个人到匹配队列，当前匹配队列人数："+matchQueue.Count);
        }

        void ReqLeaveMatch(MsgPack pack)
        {
            var list = matchQueue.ToList();
            list.RemoveAll(p=>p == pack.msg.reqLeaveMatch.uid);
            matchQueue.Clear();
            foreach(var v in list)
            {
                matchQueue.Enqueue(v);
            }

            pack.session.SendMsg(new Protocol.Msg
            {
                cmd = Protocol.CMD.RspLeaveMatch,
            });

            //更新当前uid对应的客户端的状态
            //cacheSvc.UpdateClientState(pack.msg.reqJoinMatch.uid, Protocol.Body.ClientStateEnum.None);

            this.Log("从匹配队列退出了一个，当前匹配队列人数：" + matchQueue.Count);
        }
    }
}
