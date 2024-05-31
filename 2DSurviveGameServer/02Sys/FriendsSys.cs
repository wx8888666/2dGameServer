using _2DSurviveGameServer._01Common;
using _2DSurviveGameServer._03Svc;
using _2DSurviveGameServer.Helpers;
using Microsoft.AspNetCore.Connections.Features;
using Protocol;
using Protocol.Body;
using Protocol.DBModel;
using System.Security.Cryptography;

namespace _2DSurviveGameServer._02Sys
{
    public class FriendsSys : SysRoot<FriendsSys>
    {
        private List<Friends> FriendsList;
        public override void Init()
        {
            base.Init();
            FriendsList = new List<Friends>();
           netSvc.AddMsgHandle(Protocol.CMD.ReqAddFriend, ReqAddFriend);
            netSvc.AddMsgHandle(Protocol.CMD.ReqFriends, ReqFriends);
            netSvc.AddMsgHandle(Protocol.CMD.ReqDeleFriend, ReqDeleFriend);
        }
         void ReqAddFriend(MsgPack pack)
        {
            
            ReqAddFriend reqAddFriend = pack.msg.reqAddFriend;

            var account = SqlSugarHelper.Db.Queryable<Friends>().First(p => p.UId== reqAddFriend.UId&&p.FriendUId==reqAddFriend.FriendUId);
            var res = SqlSugarHelper.Db.Queryable<User>().First(p => p.UId == reqAddFriend.FriendUId);
            bool isExist = (account != null);
            if (isExist)
            {
                return;
            }
            else
            {
                if (res != null)
                {
                     var friend = new Friends
                    {
                        UId = reqAddFriend.UId,
                        FriendUId = reqAddFriend.FriendUId,
                        Head = res.Head,
                        roleName=res.RoleName


                    };
                    var sum = SqlSugarHelper.Db.Queryable<User>().First(p => p.UId == reqAddFriend.FriendUId);
                    if (sum!=null)
                    {
                        SqlSugarHelper.Db.Insertable(friend).ExecuteCommand();
                    }
                }
                
               
            
            }
        }
       void ReqFriends(MsgPack pack)
        {
            ReqFriedns reqFriends= pack.msg.reqFriends;
            cacheSvc.UpdateFriendsList(reqFriends.UId);

            var friendsList = cacheSvc.GetFriends(reqFriends.UId);

            var rsp = new Protocol.Body.RspFriends
            {
                FriendsList = friendsList
            };
            pack.session.SendMsg(new Msg
            {
                cmd = Protocol.CMD.RspFriedns,
                rspFriedns = rsp
            }
           );
        }
        void ReqDeleFriend(MsgPack pack)
        {
            ReqDeleFriend reqDeleFriend = pack.msg.reqDeleFriend;
            bool isSuccess = SqlSugarHelper.Db.Deleteable<Friends>()
            .Where(p => p.UId == reqDeleFriend.UId && p.FriendUId == reqDeleFriend.FriendUId)
            .ExecuteCommand() > 0;
        }
    }
}
