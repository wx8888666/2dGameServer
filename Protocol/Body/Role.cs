using Protocol.DBModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Body
{

    public enum StateEnum
    {
        Alive,
        Dying,
        Dead,
    }
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
    //好友系统优化
    [Serializable]
    public class ReqAcceptFriend
    {
        public long RequestId;
    }
    [Serializable]
    public class ReqRejectFriend
    {
        public long RequestId;
    }
    [Serializable]
    public class FriendRequestNotification
    {
        public long SenderId;
    }
    [Serializable]
    public class FriendRequestAcceptedNotification
    {
        public long ReceiverId;
    }
    [Serializable]
    public class ReqGetFriendRequests
    {
        public long UserId;
    }
    [Serializable]
    public class FriendRequestDTO
    {
        public long Id { get; set; }          // 请求ID
        public long SenderId { get; set; }    // 发送者ID
        public long ReceiverId { get; set; }  // 接收者ID
        public string Status { get; set; }    // 请求状态
        public DateTime Timestamp { get; set; }  // 时间戳
    }
    [Serializable]
    public class RspGetFriendRequests
    {
        public List<FriendRequestDTO> FriendRequests;
    }
    //怪物
    [Serializable]
    public class MonsterState
    {
        public int hp;
        public int Maxhp;
        public int MonsterType;
        public long id { get; set; }
        public NetVector2 pos { get; set; }
        public NetVector2 dir { get; set; }
        public string monsterName { get; set; }
        public StateEnum state { get; set; }
    }
    [Serializable]
    public class NtfSpawnMonster
    {
        public MonsterState[] monsterStates;
    }

    [Serializable]
    public class NtfSyncMonster
    {
        public MonsterState[] monsterStates;
    }
    [Serializable]
    public class NtfMonsterHit
    {
        public MonsterState monsterState;
    }
}
