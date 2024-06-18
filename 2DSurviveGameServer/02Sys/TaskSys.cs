using _2DSurviveGameServer._01Common;
using _2DSurviveGameServer._03Svc;
using _2DSurviveGameServer.Helpers;
using Protocol.Body;
using Protocol.DBModel;
using static _2DSurviveGameServer._01Common.Friendrequest;

namespace _2DSurviveGameServer._02Sys
{
    public class TaskSys : SysRoot<TaskSys>
    {
        public override void Init()
        {
            base.Init();
            netSvc.AddMsgHandle(Protocol.CMD.ReqTask, ReqTask);
            netSvc.AddMsgHandle(Protocol.CMD.TaskChange, TaskChange);
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
        void TaskChange(MsgPack pack)
        {
            TaskChange req = pack.msg.taskChange;
            TaskSvc.Instance.UpdateTaskStatus(req.UId, req.description, req.isCompleted);
        }
        

    }
}
