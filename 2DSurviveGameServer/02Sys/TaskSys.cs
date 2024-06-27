using _2DSurviveGameServer._01Common;
using _2DSurviveGameServer._03Svc;
using _2DSurviveGameServer.Helpers;
using Protocol.Body;
using Protocol.DBModel;
using StackExchange.Redis;
using System.Reflection.Metadata.Ecma335;
using static _2DSurviveGameServer._01Common.Friendrequest;

namespace _2DSurviveGameServer._02Sys
{
    public class TaskSys : SysRoot<TaskSys>
    {
        private readonly IDatabase redisDb;
        public TaskSys()
        {
            redisDb = RedisManager.Connection.GetDatabase();
        }
        public override void Init()
        {
            base.Init();
            netSvc.AddMsgHandle(Protocol.CMD.ReqTask, ReqTask);
            netSvc.AddMsgHandle(Protocol.CMD.TaskChange, TaskChange);
            netSvc.AddMsgHandle(Protocol.CMD.ReqChangeTaskDatas, ReqChangeTaskDatas);
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
        void ReqChangeTaskDatas(MsgPack pack)
        {
            var req=pack.msg.reqChangeTaskDatas;
            var userId = req.UId;
            var taskType = req.TaskType;

            if (taskType == "AllDays")
            {
              
                long cachedTimestamp;
                var cachedTimestampString = redisDb.StringGet($"user:{userId}:lastUpdateTimestamp");

                if (!string.IsNullOrEmpty(cachedTimestampString) && long.TryParse(cachedTimestampString, out cachedTimestamp))
                {
                    
                    if ((DateTimeOffset.UtcNow.ToUnixTimeSeconds() - cachedTimestamp) < 86400)
                    {
                        Console.WriteLine("AllDays update skipped for today.");
                        return;
                    }
                }
                var currentTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                redisDb.StringSet($"user:{userId}:lastUpdateTimestamp", currentTimeStamp);
            }
            var existing = SqlSugarHelper.Db.Queryable<TaskDatas>()
                                              .First(p => p.UId == req.UId);

             TaskDatas  taskDatas;
            if (existing == null)
            {
                // 如果数据库中不存在对应 UId 的 Bag 数据，创建一个新的 Bag 对象并插入数据库
                taskDatas = new TaskDatas
                {
                    UId = req.UId,
                    DayKill = 0,
                    DayMatch = 0,
                    AllDays = 0,
                    AllKill = 0,
                };
                SqlSugarHelper.Db.Insertable(taskDatas).ExecuteCommand();
            }
            else
            {
                taskDatas = existing;
                
            }
            // 根据 TaskType 更新相应的数据
            switch (req.TaskType)
            {
                case "DayKill":
                    taskDatas.DayKill += 1;
                    taskDatas.AllKill += 1;
                    break;
                case "DayMatch":
                    taskDatas.DayMatch += 1;
                    break;
                case "AllDays":
                    taskDatas.AllDays += 1;
                    break;
                default:
                    // 处理未知的 TaskType
                    Console.WriteLine($"Unknown TaskType: {req.TaskType}");
                    break;
            }
            SqlSugarHelper.Db.Updateable(taskDatas).Where(p => p.UId == taskDatas.UId).ExecuteCommand();


        }
        void TaskChange(MsgPack pack)
        {
            TaskChange req = pack.msg.taskChange;
            var taskDescription = req.description;
            switch (taskDescription)
            {
                case "每日登录":
                    HandleDailyLogin(req.UId);
                    break;
                case "每日游戏":
                    HandlePlayGame(req.UId);
                    break;
                case "友情延续":
                    HandleChatWithFriend(req.UId);
                    break;
                case "奋勇杀敌":
                    HandleKillEnemy(req.UId);
                    break;
                case "战场名将":
                    HandlePlayGamesCount(req.UId);
                    break;
            }
        }
        
        void HandleDailyLogin(long userId)
        {
            TaskSvc.Instance.UpdateTaskStatus(userId, "每日登录", true);
        }

        void HandlePlayGame(long userId)
        {
            TaskSvc.Instance.UpdateTaskStatus(userId, "每日游戏", true);
        }

        void HandleChatWithFriend(long userId)
        {
            TaskSvc.Instance.UpdateTaskStatus(userId, "友情延续", true);
        }

        void HandleKillEnemy(long userId)
        {
            var existing = SqlSugarHelper.Db.Queryable<TaskDatas>()
                                            .First(p => p.UId == userId);
            if (existing == null) return;
            else {
                if (existing.DayKill >= 10)
                {
                    TaskSvc.Instance.UpdateTaskStatus(userId, "奋勇杀敌", true);
                }
            }
               
        }

        void HandlePlayGamesCount(long userId)
        {
            var existing = SqlSugarHelper.Db.Queryable<TaskDatas>()
                                           .First(p => p.UId == userId);
            if (existing == null) return;
            else
            {
                if (existing.DayMatch >= 10)
                {
                    TaskSvc.Instance.UpdateTaskStatus(userId, "战场名将", true);
                }
            }
            
        }


    }
}
