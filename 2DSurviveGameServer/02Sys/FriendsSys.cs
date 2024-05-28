using _2DSurviveGameServer._01Common;
using _2DSurviveGameServer._03Svc;
using _2DSurviveGameServer.Helpers;
using Protocol;
using Protocol.Body;
using Protocol.DBModel;

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
        }
         void ReqAddFriend(MsgPack pack)
        {
            ReqAddFriend reqAddFriend = pack.msg.reqAddFriend;
            
            var friend = new Friends
            {
                UId = reqAddFriend.UId,
                FriendUId = reqAddFriend.FriendUId,
            };

            SqlSugarHelper.Db.Insertable(friend).ExecuteCommand();
            
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
    }
}
