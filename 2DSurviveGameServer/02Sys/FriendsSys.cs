using _2DSurviveGameServer._01Common;
using _2DSurviveGameServer._03Svc;
using _2DSurviveGameServer.Helpers;
using Microsoft.AspNetCore.Connections.Features;
using Protocol;
using Protocol.Body;
using Protocol.DBModel;
using System.Security.Cryptography;
using static _2DSurviveGameServer._01Common.Friendrequest;

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
            netSvc.AddMsgHandle(Protocol.CMD.ReqAcceptFriend, ReqAcceptFriend);
            netSvc.AddMsgHandle(Protocol.CMD.ReqRejectFriend, ReqRejectFriend);
            netSvc.AddMsgHandle(Protocol.CMD.ReqGetFriendRequests, ReqGetFriendRequests);
        }

        // 发送好友请求
        void ReqAddFriend(MsgPack pack)
        {
            ReqAddFriend reqAddFriend = pack.msg.reqAddFriend;

            var existingRequest = SqlSugarHelper.Db.Queryable<FriendRequests>()
                .First(p => p.SenderId == reqAddFriend.UId && p.ReceiverId == reqAddFriend.FriendUId && p.Status == "pending");

            if (existingRequest != null)
            {
                return; // 已经有待处理的请求
            }

            var friendRequest = new FriendRequests
            {
                SenderId = reqAddFriend.UId,
                ReceiverId = reqAddFriend.FriendUId,
                Status = "pending"
            };

            SqlSugarHelper.Db.Insertable(friendRequest).ExecuteCommand();
 
        }

        // 接受好友请求
        void ReqAcceptFriend(MsgPack pack)
        {
            ReqAcceptFriend reqAcceptFriend = pack.msg.reqAcceptFriend;

            var friendRequest = SqlSugarHelper.Db.Queryable<FriendRequests>()
                .First(p => p.Id == reqAcceptFriend.RequestId && p.Status == "pending");

            if (friendRequest == null)
            {
                return; // 请求不存在或已处理
            }

            var sender = SqlSugarHelper.Db.Queryable<User>().First(p => p.UId == friendRequest.SenderId);
            var receiver = SqlSugarHelper.Db.Queryable<User>().First(p => p.UId == friendRequest.ReceiverId);

            if (sender != null && receiver != null)
            {
                var friend1 = new Friends
                {
                    UId = friendRequest.SenderId,
                    FriendUId = friendRequest.ReceiverId,
                    Head = receiver.Head,
                    roleName = receiver.RoleName
                };

                var friend2 = new Friends
                {
                    UId = friendRequest.ReceiverId,
                    FriendUId = friendRequest.SenderId,
                    Head = sender.Head,
                    roleName = sender.RoleName
                };

                SqlSugarHelper.Db.Insertable(friend1).ExecuteCommand();
                SqlSugarHelper.Db.Insertable(friend2).ExecuteCommand();

                // 更新好友请求状态
                friendRequest.Status = "accepted";
                SqlSugarHelper.Db.Updateable(friendRequest).ExecuteCommand();
               
                // 通知发送者请求已被接受
                //NotifyUserRequestAccepted(friendRequest.SenderId, friendRequest.ReceiverId);
            }
        }

        // 拒绝好友请求
        void ReqRejectFriend(MsgPack pack)
        {
            ReqRejectFriend reqRejectFriend = pack.msg.reqRejectFriend;

            var friendRequest = SqlSugarHelper.Db.Queryable<FriendRequests>()
                .First(p => p.Id == reqRejectFriend.RequestId && p.Status == "pending");

            if (friendRequest != null)
            {
                friendRequest.Status = "rejected";
                SqlSugarHelper.Db.Updateable(friendRequest).ExecuteCommand();
            }
        }

        // 获取好友列表
        void ReqFriends(MsgPack pack)
        {
            ReqFriedns reqFriends = pack.msg.reqFriends;
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
            });
        }

        // 删除好友
        void ReqDeleFriend(MsgPack pack)
        {
            ReqDeleFriend reqDeleFriend = pack.msg.reqDeleFriend;

            bool isSuccess1 = SqlSugarHelper.Db.Deleteable<Friends>()
                .Where(p => p.UId == reqDeleFriend.UId && p.FriendUId == reqDeleFriend.FriendUId)
                .ExecuteCommand() > 0;

            bool isSuccess2 = SqlSugarHelper.Db.Deleteable<Friends>()
                .Where(p => p.UId == reqDeleFriend.FriendUId && p.FriendUId == reqDeleFriend.UId)
                .ExecuteCommand() > 0;
        }
   
        // 获取用户的未读通知
        void ReqGetFriendRequests(MsgPack pack)
        {
            ReqGetFriendRequests reqGetNotifications = pack.msg.reqGetFriendRequests;

            // 查询处于 pending 状态的好友请求
            var pendingFriendRequests = SqlSugarHelper.Db.Queryable<FriendRequests>()
                .Where(fr => fr.ReceiverId == reqGetNotifications.UserId && fr.Status == "pending")
                .ToList();

            // 将查询到的请求转换为适当的 DTO 对象
            var friendRequestDTOs = pendingFriendRequests.Select(fr => new FriendRequestDTO
            {
                Id = fr.Id,
                SenderId = fr.SenderId,
                ReceiverId = fr.ReceiverId,
                Status = fr.Status,
                Timestamp = fr.Timestamp
            }).ToList();

            // 创建包含 DTO 对象的响应对象
            var rsp = new Protocol.Body.RspGetFriendRequests
            {
                FriendRequests = friendRequestDTOs
            };

            // 发送响应给客户端
            pack.session.SendMsg(new Msg
            {
                cmd = Protocol.CMD.RspGetFriendRequests,
                rspGetFriendRequests = rsp
            });

            // （可选）将已发送的请求标记为已读或进行其他处理
            // 标记已读可根据具体需求来决定，如果只在服务端处理好友请求，可以不需要标记为已读
        }



    }
}
