using _2DSurviveGameServer._02Sys;
using _2DSurviveGameServer._02Sys.Room;
using _2DSurviveGameServer._03Svc;
using _2DSurviveGameServer._04Interface;
using _2DSurviveGameServer.Helpers;
using PEUtils;
using Protocol.DBModel;
using SqlSugar;
using Yitter.IdGenerator;
using static _2DSurviveGameServer._01Common.Friendrequest;

namespace _2DSurviveGameServer._01Common
{
    public class ServerRoot : Singleton<ServerRoot>, IComponent
    {
        public void Init()
        {
            PELog.InitSettings();

            YitIdHelper.SetIdGenerator(new IdGeneratorOptions());
            StaticConfig.CustomSnowFlakeFunc = () =>
            {
                return YitIdHelper.NextId();
            };
            SqlSugarHelper.Db.DbMaintenance.CreateDatabase();
            SqlSugarHelper.Db.CodeFirst.InitTables<Account>();
            SqlSugarHelper.Db.CodeFirst.InitTables<User>();
            SqlSugarHelper.Db.CodeFirst.InitTables<Friends>();
            SqlSugarHelper.Db.CodeFirst.InitTables<FriendRequests>();
            SqlSugarHelper.Db.CodeFirst.InitTables<TaskType>();
            SqlSugarHelper.Db.CodeFirst.InitTables<Bag>();
            //SqlSugarHelper.Db.CodeFirst.InitTables<Notifications>();
          

            NetSvc.Instance.Init();
            CacheSvc.Instance.Init();
            TimerSvc.Instance.Init();

            LoginSys.Instance.Init();
            PingSys.Instance.Init();
            CreateRoleSys.Instance.Init();
            LobbySys.Instance.Init();
            RoomSys.Instance.Init();
            RegSys.Instance.Init();
            ReconnectSys.Instance.Init();
            FriendsSys.Instance.Init();
            ChatSys.Instance.Init();
            TaskSvc.Instance.Init();
            TaskSys.Instance.Init();
        }

        public void Update()
        {
            NetSvc.Instance.Update();
            CacheSvc.Instance.Update();
            TimerSvc.Instance.Update();

            LoginSys.Instance.Update();
            PingSys.Instance.Update();
            CreateRoleSys.Instance.Update();
            LobbySys.Instance.Update();
            RoomSys.Instance.Update();
            RegSys.Instance.Update();
            ReconnectSys.Instance.Update();
        }
    }
}
