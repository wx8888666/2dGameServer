using _2DSurviveGameServer._01Common;
using _2DSurviveGameServer._03Svc;
using _2DSurviveGameServer.Helpers;
using Protocol.Body;
using Protocol.DBModel;

namespace _2DSurviveGameServer._02Sys
{
    public class CreateRoleSys:SysRoot<CreateRoleSys>
    {
        public override void Init()
        {
            base.Init();
            netSvc.AddMsgHandle(Protocol.CMD.ReqCreateRole, ReqCreateRole);
        }

        public override void Update()
        {
            base.Update();
        }

        void ReqCreateRole(MsgPack pack)
        {
            RspCreateRole rsp = new RspCreateRole();
            if(SqlSugarHelper.Db.Queryable<User>().First(p => p.RoleName == pack.msg.reqCreateRole.roleName) != null)
            {
                //已经存在相同名字的角色了
                rsp.createRoleEnum = CreateRoleEnum.Failed;
                rsp.error = "已经有相同名字的角色了";
            }
            else
            {
                string head = pack.msg.reqCreateRole.head;
                //把头像移动到永久保存的目录
                if (File.Exists(AppContext.BaseDirectory + "wwwroot\\temp\\" + head))
                {
                    Directory.CreateDirectory(AppContext.BaseDirectory + "wwwroot\\head\\");//有可能这个目录不存在，所以需要创建
                    //如果temp缓存目录下存在那么就移动到用久目录
                    File.Move(AppContext.BaseDirectory + "wwwroot\\temp\\" + head, AppContext.BaseDirectory + "wwwroot\\head\\" + head);

                    User user = new User
                    {
                        UId = pack.msg.reqCreateRole.uid,
                        RoleId = pack.msg.reqCreateRole.roleId,
                        Head = pack.msg.reqCreateRole.head,
                        RoleName = pack.msg.reqCreateRole.roleName,
                    };
                    SqlSugarHelper.Db.Insertable<User>(user).ExecuteReturnSnowflakeId();

                    rsp.createRoleEnum = CreateRoleEnum.Success;
                    rsp.user = user;
                }
                else
                {
                    //出现异常，temp目录下不存在
                    rsp.createRoleEnum = CreateRoleEnum.Failed;
                    rsp.error = "服务器发生异常，上传的头像图片不存在，请重新上传头像文件！";
                }
            }



            pack.session.SendMsg(new Protocol.Msg
            {
                cmd = Protocol.CMD.RspCreateRole,
                rspCreateRole = rsp,
            });
        }
    }
}
