using _2DSurviveGameServer._01Common;
using _2DSurviveGameServer._03Svc;
using _2DSurviveGameServer.Helpers;
using Protocol;
using Protocol.Body;
using Protocol.DBModel;
using System.Security.Cryptography;

namespace _2DSurviveGameServer._02Sys
{
    public class LoginSys:SysRoot<LoginSys>
    {
        public override void Init()
        {
            base.Init();

            netSvc.AddMsgHandle(Protocol.CMD.ReqLogin, ReqLogin);
            netSvc.AddMsgHandle(Protocol.CMD.ReqLogout, ReqLogout);
        }

        public override void Update()
        {
            base.Update();
        }

        /// <summary>
        /// 客户端请求登录
        /// </summary>
        /// <param name="pack"></param>
        void ReqLogin(MsgPack pack)
        {
            ReqLogin req = pack.msg.reqLogin;
            RspLogin rsp = new RspLogin();
            Account account = SqlSugarHelper.Db.Queryable<Account>().First(p=>p.Username == req.username && p.Password == req.password);
            if(account == null)
            {
                //登录失败，账号密码不匹配
                rsp.loginEnum = LoginEnum.Failed;
                rsp.error = "账号或密码不正确";
            }
            else
            {
                rsp.loginEnum = LoginEnum.Success;
                //返回该账号的uid和角色以及其他。
                User user = SqlSugarHelper.Db.Queryable<User>().First(p => p.UId == account.Id);
                rsp.user = user;
                rsp.uid = account.Id;//返回uid
                cacheSvc.UpdateUidUser(account.Id, user);
                cacheSvc.UpdateUserHeartbeat(account.Id, pack.session);
                //SendTasksToClient(pack);
            }

            pack.session.SendMsg(new Protocol.Msg
            {
                cmd = Protocol.CMD.RspLogin,
                rspLogin = rsp,
            });
        }


        void ReqLogout(MsgPack pack)
        {
            cacheSvc.RemoveUser(pack.msg.reqLogout.uid);

            pack.session.SendMsg(new Protocol.Msg
            {
                cmd = Protocol.CMD.RspLogout,
            });
        }
        //新增任务发送
        void SendTasksToClient(MsgPack pack)
        {
            // 获取任务列表
            List<GameTask> gameTasks = GameTaskSys.Instance.GetTasks();

            // 初始化RspTask并转换GameTask到GTask的数组
            RspTask rspTask = new RspTask();
            rspTask.tasks = gameTasks.Select(v => new GTask
            {
                Name = v.Name,
                Task = v.Task,
                Reward = v.Reward
            }).ToArray(); // 使用LINQ Select转换并转为数组

            // 发送消息给客户端
            pack.session.SendMsg(new Protocol.Msg
            {
                cmd = Protocol.CMD.RspTask,
                rspTask = rspTask,
            });
        }
    }
}
