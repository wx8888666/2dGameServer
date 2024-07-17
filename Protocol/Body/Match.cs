using Protocol.DBModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Body
{
    [Serializable]
    public class ReqJoinMatch
    {
        public long uid;
    }

    [Serializable]
    public class ReqLeaveMatch
    {
        public long uid;
    }

    [Serializable]
    public class NtfMatchComplete
    {
        public long roomId;
        public User[] userArr;
    }

    [Serializable]
    public class NtfConfirm
    {
        public bool[] confirmArr;
    }

    [Serializable]
    public class SndConfirm
    {
        public long roomId;
        public long uid;
    }
    [Serializable]
    public class NtfRoleInfor
    {
        public RoleState roleState;
    }
    //退出游戏
    [Serializable]
    public class ReqExitBattle
    {
        public long uid;
        public long roomId;
    }
    [Serializable]
    public class RspExitBattle
    {

    }
}
