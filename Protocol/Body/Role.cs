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
    public class ActorObject
    {
        public long id;

        public NetVector2 pos;
    }

    [Serializable]
    public class RoleState:ActorObject
    {
        public long uid;

        public string roleName;

        public int speed;

        public int hp;

        public int maxHp;

        public int def;

        public NetVector2 dir;

        public NetVector2 mousePos;

        public int exp;

        public int maxExp;

        public bool isKing = false;

        public WeaponObject weaponObject;
    }
    

    [Serializable]
    public class NtfSpawnRole
    {
        public RoleState[] roleStates;
    }

    [Serializable]
    public class NtfSyncRole
    {
        public RoleState [] roleStates;
    }

    [Serializable]
    public class SndRoleState
    {
        public long uid;
        public long roomId;
        public RoleState roleState;
    }
    //好友功能
    [Serializable]
    public class RspFriends
    {
        public Friends[] FriendsList;
    }
    [Serializable]
    public class ReqFriedns
    {
        public long UId;
    }
    [Serializable]
    public class ReqAddFriend
    {
        public long UId;
        public long FriendUId;
    }
    [Serializable]
    public class ReqDeleFriend
    {
        public long UId;
        public long FriendUId;
    }
    [Serializable]
    public class UserOnline
    {
        public long UId;
    }
    [Serializable]
    public class ReqChatHistory
    {
        public long uId;
        public long friendUId;
    }
}
