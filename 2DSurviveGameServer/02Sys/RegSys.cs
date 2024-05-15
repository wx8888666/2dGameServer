using _2DSurviveGameServer._01Common;
using _2DSurviveGameServer._03Svc;
using _2DSurviveGameServer.Helpers;
using Protocol.Body;
using Protocol.DBModel;

namespace _2DSurviveGameServer._02Sys
{
    public class RegSys : SysRoot<RegSys>
    {
        public override void Init()
        {
            base.Init();
            netSvc.AddMsgHandle(Protocol.CMD.ReqReg, ReqReg);
        }
        void ReqReg(MsgPack pack)
        {
            ReqReg req = pack.msg.reqReg;
            RspReg rsp = new RspReg();
            var account = SqlSugarHelper.Db.Queryable<Account>().First(p => p.Username == req.Username);
            var count = SqlSugarHelper.Db.Queryable<Account>().Count();
            bool isExist = (account != null);
            if (isExist)
            {
                rsp.LoginEnum = LoginEnum.Failed;
                rsp.Error = "账号已经存在";
            }
            else
            {
                var reqAccount = new Account
                {
                    Id = count,
                    Username = req.Username,
                    Password = req.Password,
                };

                var result = SqlSugarHelper.Db.Insertable(reqAccount).ExecuteCommand();
                if (result > 0)
                {
                    // 插入成功
                    Console.WriteLine("数据插入成功！");
                    rsp.LoginEnum = LoginEnum.Success;
                }
                else
                {
                    // 插入失败
                    Console.WriteLine("数据插入失败！");
                }

            }
            pack.session.SendMsg(msg: new Protocol.Msg
            {
                cmd = Protocol.CMD.RspReg,
                rspReg = rsp,
            });
        }
    
   }
}
