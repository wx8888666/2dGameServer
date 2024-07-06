using PENet;
using Protocol.Body;
using Protocol.DBModel;
using System;

namespace Protocol
{
    [Serializable]//标注上可以被序列化
    public class Msg:KCPMsg  //消息包类了
    {
        public CMD cmd;//指令，用于判定这条消息是干什么用的
        public ReqPing reqPing;
        public RspPing rspPing;
        public ReqLogin reqLogin;
        public RspLogin rspLogin;
        public ReqCreateRole reqCreateRole;
        public RspCreateRole rspCreateRole;
        public ReqLogout reqLogout;
        public ReqJoinMatch reqJoinMatch;
        public ReqLeaveMatch reqLeaveMatch;
        public NtfMatchComplete ntfMatchComplete;
        public NtfConfirm ntfConfirm;
        public SndConfirm sndConfirm;
        public SndLoadPrg sndLoadPrg;
        public NtfLoadPrg ntfLoadPrg;
        public NtfSpawnRole ntfSpawnRole;
        public NtfSyncRole ntfSyncRole;
        public SndRoleState sndRoleState;
        public ReqClientState reqClientState;
        public RspClientState rspClientState;
        public SndEnterRoom sndEnterRoom;
        public ReqReg reqReg;
        public RspReg rspReg;
        public ReqTask reqTask;
        public RspTask rspTask;
        public ReqFriedns reqFriends;
        public RspFriends rspFriedns;
        public ReqAddFriend reqAddFriend;
        public ReqDeleFriend reqDeleFriend;
        public NtfSpawnWeapon ntfSpawnWeapon;
        public ChatMessage chatMessage;
        public RspChatMessage rspChatMessage;
        public UserOnline userOnline;
        public ReqChatHistory reqChatHistory;
        public ReqAcceptFriend reqAcceptFriend;
        public ReqRejectFriend reqRejectFriend;
        public FriendRequestNotification friendRequestNotification;
        public FriendRequestAcceptedNotification friendRequestAcceptedNotification;
        public ReqGetFriendRequests reqGetFriendRequests;
        public RspGetFriendRequests rspGetFriendRequests;
        public ReqBag reqBag;
        public RspBag rspBag;
        public TaskChange taskChange;
        public NtfSpawnMonster ntfSpawnMonster;
        public NtfSyncMonster ntfSyncMonster;
        public ReqChangeTaskDatas reqChangeTaskDatas;
        public ReqPickupWeapon reqPickupWeapon;
        public RspPickupWeapon rspPickupWeapon;
        public ReqWeaponFire reqWeaponFire;
        public RspWeaponFire rspWeaponFire;
        public NtfMonsterHit ntfMonsterHit;
    }

}
