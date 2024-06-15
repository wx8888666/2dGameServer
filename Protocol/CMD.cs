using Protocol.Body;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol
{
    public enum CMD
    {
        ReqPing,
        RspPing,
        ReqLogin,
        RspLogin,
        ReqCreateRole,
        RspCreateRole,
        ReqLogout,
        RspLogout,
        ReqJoinMatch,
        RspJoinMatch,
        ReqLeaveMatch,
        RspLeaveMatch,
        NtfMatchComplete,
        NtfMatchDisband,
        NtfConfirm,
        SndConfirm,
        NtfStartLoad,
        SndLoadPrg,
        NtfLoadPrg,
        NtfStartFight,
        NtfSpawnRole,
        NtfSyncRole,
        SndRoleState,
        ReqClientState,
        RspClientState,
        SndEnterRoom,
        ReqReg,
        RspReg,
        ReqTask,
        RspTask,
        ReqFriends,
        RspFriedns,
        ReqAddFriend,
        ReqDeleFriend,
        NtfSpawnWeapon,
        ChatMessage,
        RspChatMessage,
        UserOnline,
        ReqChatHistory,
        ReqAcceptFriend,
        ReqRejectFriend,
        FriendRequestNotification,
        FriendRequestAcceptedNotification,
        ReqGetFriendRequests,
        RspGetFriendRequests,
        ReqBag,
        RspBag,
}
}
