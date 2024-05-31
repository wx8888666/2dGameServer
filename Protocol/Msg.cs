using PENet;
using Protocol.Body;
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
        public RspTask rspTask;
        public ReqFriedns reqFriends;
        public RspFriends rspFriedns;
        public ReqAddFriend reqAddFriend;
        public ReqDeleFriend reqDeleFriend;
    }

}
