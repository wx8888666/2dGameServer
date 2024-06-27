using _2DSurviveGameServer.Helpers;
using Protocol.DBModel;
using Quartz;
using Quartz.Impl;
using SqlSugar;

namespace _2DSurviveGameServer._01Common
{
    public class ResetDailyStatusJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {

            SqlSugarHelper.Db.Updateable<TaskDatas>()
            .SetColumns(it => new TaskDatas { DayKill = 0, DayMatch = 0 })
            .ExecuteCommand();

            Console.WriteLine("Daily status has been reset.");
            return Task.CompletedTask;
        }
    }
   

}
