using Protocol.DBModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Body
{
    [Serializable]
    public class ReqCreateRole
    {
        public long uid;
        public int roleId;
        public string roleName;
        public string head;
    }

    [Serializable]
    public class RspCreateRole
    {
        public User user;
        public CreateRoleEnum createRoleEnum;
        public string error;
    }

    public enum CreateRoleEnum
    {
        Success,
        Failed
    }



    [Serializable]
    public class RoleState
    {
        public long uid;
        public string roleName;
        public NetVector2 pos;
        public NetVector2 dir;
    }


    [Serializable]
    public class NtfSpawnRole
    {
        public RoleState[] roleStates;
    }

    [Serializable]
    public class NtfSyncRole
    {
        public RoleState[] roleStates;
    }

    [Serializable]
    public class SndRoleState
    {
        public long uid;
        public long roomId;
        public RoleState roleState;
    }
}
