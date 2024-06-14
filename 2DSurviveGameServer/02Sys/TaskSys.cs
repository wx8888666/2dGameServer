using _2DSurviveGameServer._01Common;
using _2DSurviveGameServer._03Svc;
using _2DSurviveGameServer.Helpers;
using Protocol.Body;
using static _2DSurviveGameServer._01Common.Friendrequest;

namespace _2DSurviveGameServer._02Sys
{
    public class TaskSys : SysRoot<TaskSys>
    {
        public override void Init()
        {
            base.Init();
            netSvc.AddMsgHandle(Protocol.CMD.ReqTask, ReqTask);
        }
        public override void Update()
        {
            base.Update();
        }
        void ReqTask(MsgPack pack)
        {
            TaskSvc.Instance.AssignDailyTasks(pack.msg.reqTask.uid);
            var Tasks = TaskSvc.Instance.GetUserTasks(pack.msg.reqTask.uid);
            pack.session.SendMsg(new Protocol.Msg
            {
                cmd = Protocol.CMD.RspTask,
                rspTask = new RspTask
                {
                    UserTask = Tasks.ToArray()
                }
            });
        }
        

    }
}
